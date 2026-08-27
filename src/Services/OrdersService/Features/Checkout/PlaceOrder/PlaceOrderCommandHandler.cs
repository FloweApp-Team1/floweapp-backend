using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Features.Checkout.Common;
using OrdersService.Features.Checkout.PlaceOrder;
using OrdersService.Infrastructure.Services;
using Shared.Events.OrderEvents;
using Shared.Interfaces;
using Shared.Results;
public sealed class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResponse?>>
{
    private const string DefaultCurrency = "egp"; 

    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAddressServiceClient _addressServiceClient;
    private readonly ICheckoutPricingService _pricingService;
    private readonly IPaymentSessionClient _paymentSessionClient;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public PlaceOrderCommandHandler(
        ISender sender,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAddressServiceClient addressServiceClient,
        ICheckoutPricingService pricingService,
        IPaymentSessionClient paymentSessionClient,
        IIdempotencyService idempotencyService,
        IIntegrationEventPublisher eventPublisher)
    {
        _sender = sender;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _addressServiceClient = addressServiceClient;
        _pricingService = pricingService;
        _paymentSessionClient = paymentSessionClient;
        _idempotencyService = idempotencyService;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<PlaceOrderResponse?>> Handle(
        PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result.Failure<PlaceOrderResponse?>(Error.New("Order.Unauthorized", "User is not authenticated."));

        // 1) Idempotency 
        var cached = await _idempotencyService.GetCachedResponseAsync<IdempotentPlaceOrderResult>(
            userId.Value, request.IdempotencyKey, cancellationToken);

        if (cached is not null)
            return Result.Success(cached.Data);

        // 2) addressId 
        var addressResult = await _addressServiceClient.GetAddressForOrderAsync(
            request.AddressId, userId.Value, cancellationToken);

        if (addressResult.IsFailure)
            return Result.Failure<PlaceOrderResponse?>(addressResult.Error);

        var address = addressResult.Value;

        if (!address.IsServiceable || address.StoreId is null)
            return Result.Failure<PlaceOrderResponse?>(Error.New(
                "Order.NotServiceable", "This address is outside our current delivery coverage."));

        // 3) Pricing 
        var pricingResult = await _pricingService.CalculateAsync(request.CartId, userId.Value, address, cancellationToken);
        if (pricingResult.IsFailure)
            return Result.Failure<PlaceOrderResponse?>(pricingResult.Error);

        var pricing = pricingResult.Value;

        // 4) create Order Aggregate (in-memory) 
        var order = BuildOrder(request, userId.Value, address, pricing);

        // 5) create Order Aggregate (persisted) 
        var createResult = await _sender.Send(new CreateOrderCommand(order), cancellationToken);
        if (createResult.IsFailure)
            return Result.Failure<PlaceOrderResponse?>(createResult.Error);

        // 6)  Payment Method 
        var responseResult = request.PaymentMethod == PaymentMethodEnum.Cod
            ? await HandleCodAsync(order, userId.Value, cancellationToken)
            : await HandleCardAsync(order, request.PaymentGateway!, pricing.EstimatedDeliveryAt, cancellationToken);

        if (responseResult.IsFailure)
            return Result.Failure<PlaceOrderResponse?>(responseResult.Error);

        // 7) Idempotency Store Response
        await _idempotencyService.StoreResponseAsync(
            userId.Value, request.IdempotencyKey,
            new IdempotentPlaceOrderResult(order.Id, responseResult.Value), cancellationToken);

        return Result.Success(responseResult.Value);
    }

    // Order Assembly 

    private static Order BuildOrder(
        PlaceOrderCommand request, Guid userId, OrderAddressDetails address, CheckoutPricingResult pricing)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            StoreId = address.StoreId!.Value,
            AddressId = address.AddressId,
            PaymentMethod = request.PaymentMethod,
            Status = OrderStatusEnum.Placed,
            PaymentStatus = PaymentStatusEnum.Pending,
            Subtotal = pricing.Subtotal,
            DeliveryFee = pricing.DeliveryFee,
            Total = pricing.Total
        };

        foreach (var item in pricing.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductImageUrl = item.ProductImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            });
        }

        if (request.IsGift)
        {
            order.IsGift = true;
            order.GiftRecipientName = request.GiftRecipient!.RecipientName;
            order.GiftRecipientPhone = request.GiftRecipient.RecipientPhone;
          
        }

        order.AddressSnapshot = new OrderAddressSnapshot
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            RecipientName = address.RecipientName,
            RecipientPhone = address.RecipientPhone,
            AddressLine = address.AddressLine,
            City = address.CityName,
            Area = address.Area,
            Lat = address.Lat,
            Lng = address.Lng
        };

        return order;
    }

    // COD Path

    private async Task<Result<PlaceOrderResponse?>> HandleCodAsync(
        Order order, Guid userId, CancellationToken cancellationToken)
    {
       
        await _eventPublisher.PublishAsync(
            new OrderConfirmedEvent
            {
                OrderId = order.Id,
                UserId = userId,
                PaymentMethod = order.PaymentMethod.ToString(),
                OrderNumber = order.OrderNumber,
                Total = order.Total,
                UserEmail = _currentUserService.Email
            },
            cancellationToken);

       
        return Result.Success<PlaceOrderResponse?>(null);
    }

    // Card Path

    private async Task<Result<PlaceOrderResponse?>> HandleCardAsync(
        Order order, string gateway, DateTime? estimatedDeliveryAt, CancellationToken cancellationToken)
    {
        var amountTotalCents = (long)Math.Round(order.Total * 100);

        var sessionResult = await _paymentSessionClient.CreateCheckoutSessionAsync(
            order.Id, amountTotalCents, DefaultCurrency, cancellationToken);

        if (sessionResult.IsFailure)
            return Result.Failure<PlaceOrderResponse?>(sessionResult.Error);

        var session = sessionResult.Value;

     
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var orderRepository = _unitOfWork.Repository<Order>();
            var trackedOrder = await orderRepository.GetByIdAsync(order.Id, cancellationToken);

            if (trackedOrder is not null)
            {
                trackedOrder.LastPaymentAttemptId = session.PaymentAttemptId;
                orderRepository.Update(trackedOrder);
            }

            return Result.Success();
        }, cancellationToken);

        var response = new PlaceOrderResponse(
            order.Id,
            order.Status.ToString(),
            gateway,
            session.SessionId,
            session.SessionUrl,
            session.SuccessUrl,
            session.CancelUrl,
            session.ExpiresAt?? DateTime.UtcNow.AddHours(1),
            order.Total,
            DefaultCurrency.ToUpperInvariant(),
            estimatedDeliveryAt ?? DateTime.UtcNow);

        return Result.Success<PlaceOrderResponse?>(response);
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
    private sealed record IdempotentPlaceOrderResult(Guid OrderId, PlaceOrderResponse? Data);
}