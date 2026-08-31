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

        // 1) Reserve the idempotency key atomically before any business logic runs.
        var reservation = await _idempotencyService.TryReserveAsync<IdempotentPlaceOrderResult>(
            userId.Value, request.IdempotencyKey, cancellationToken);

        if (reservation.AlreadyCompleted)
            return Result.Success(reservation.CachedResult!.Data);

        if (!reservation.Acquired)
            return Result.Failure<PlaceOrderResponse?>(Error.New(
                "Order.DuplicateRequest", "This order is already being processed. Please wait."));

        try
        {
            // 2) Address
            var addressResult = await _addressServiceClient.GetAddressForOrderAsync(
                request.AddressId, userId.Value, cancellationToken);

            if (addressResult.IsFailure)
            {
                await _idempotencyService.ReleaseReservationAsync(userId.Value, request.IdempotencyKey, cancellationToken);
                return Result.Failure<PlaceOrderResponse?>(addressResult.Error);
            }

            var address = addressResult.Value;

            if (!address.IsServiceable || address.StoreId is null)
            {
                await _idempotencyService.ReleaseReservationAsync(userId.Value, request.IdempotencyKey, cancellationToken);
                return Result.Failure<PlaceOrderResponse?>(Error.New(
                    "Order.NotServiceable", "This address is outside our current delivery coverage."));
            }

            // 3) Pricing
            var pricingResult = await _pricingService.CalculateAsync(request.CartId, userId.Value, address, cancellationToken);
            if (pricingResult.IsFailure)
            {
                await _idempotencyService.ReleaseReservationAsync(userId.Value, request.IdempotencyKey, cancellationToken);
                return Result.Failure<PlaceOrderResponse?>(pricingResult.Error);
            }

            var pricing = pricingResult.Value;

            // 4) Build the order in memory
            var order = BuildOrder(request, userId.Value, address, pricing);

            // 5) Card only: get the payment session before persisting
            PlaceOrderResponse? response = null;

            if (request.PaymentMethod == PaymentMethodEnum.Card)
            {
                var sessionResult = await _paymentSessionClient.CreateCheckoutSessionAsync(
                    order.Id, (long)Math.Round(order.Total * 100), DefaultCurrency, cancellationToken);

                if (sessionResult.IsFailure)
                {
                    await _idempotencyService.ReleaseReservationAsync(userId.Value, request.IdempotencyKey, cancellationToken);
                    return Result.Failure<PlaceOrderResponse?>(sessionResult.Error);
                }

                var session = sessionResult.Value;
                order.LastPaymentAttemptId = session.PaymentAttemptId;

                response = new PlaceOrderResponse(
                    order.Id,
                    order.Status.ToString(),
                    request.PaymentGateway!,
                    session.SessionId,
                    session.SessionUrl,
                    session.SuccessUrl,
                    session.CancelUrl,
                    session.ExpiresAt ?? DateTime.UtcNow.AddHours(1),
                    order.Total,
                    DefaultCurrency.ToUpperInvariant(),
                    pricing.EstimatedDeliveryAt ?? DateTime.UtcNow);
            }

            // 6) Persist Order + Publish OrderConfirmedEvent for COD orders,
            // both inside the same UnitOfWork.ExecuteAsync so the EF Core
            // Outbox captures the message on the same SaveChangesAsync.
            var persistResult = await _unitOfWork.ExecuteAsync(async () =>
            {
                var createResult = await _sender.Send(new CreateOrderCommand(order), cancellationToken);
                if (createResult.IsFailure)
                    return Result.Failure(createResult.Error);

                if (request.PaymentMethod == PaymentMethodEnum.Cod)
                {
                    await _eventPublisher.PublishAsync(
                        new OrderConfirmedEvent
                        {
                            OrderId = order.Id,
                            UserId = userId.Value,
                            PaymentMethod = order.PaymentMethod.ToString(),
                            OrderNumber = order.OrderNumber,
                            Total = order.Total,
                            UserEmail = _currentUserService.Email
                        },
                        cancellationToken);
                }

                return Result.Success();
            }, cancellationToken);

            if (persistResult.IsFailure)
            {
                await _idempotencyService.ReleaseReservationAsync(userId.Value, request.IdempotencyKey, cancellationToken);
                return Result.Failure<PlaceOrderResponse?>(persistResult.Error);
            }

            // 7) Complete the reservation with the final result
            await _idempotencyService.CompleteReservationAsync(
                userId.Value, request.IdempotencyKey,
                new IdempotentPlaceOrderResult(order.Id, response), cancellationToken);

            return Result.Success(response);
        }
        catch
        {
            // Unexpected failure after reserving - release so the user can retry
            // instead of being stuck on "already in progress" until the TTL expires.
            await _idempotencyService.ReleaseReservationAsync(userId.Value, request.IdempotencyKey, cancellationToken);
            throw;
        }
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

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

    private sealed record IdempotentPlaceOrderResult(Guid OrderId, PlaceOrderResponse? Data);
}