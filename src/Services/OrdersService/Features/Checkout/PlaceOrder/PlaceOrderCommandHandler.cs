using MediatR;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Clients;
using OrdersService.Infrastructure.Services;
using Shared.Events.OrderEvents;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed class PlaceOrderCommandHandler
        : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResponse>>
    {
        private readonly ISender _sender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICartServiceClient _cartServiceClient;
        private readonly IAddressServiceClient _addressServiceClient;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IDeliveryFeeCalculator _deliveryFeeCalculator;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public PlaceOrderCommandHandler(
            ISender sender,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICartServiceClient cartServiceClient,
            IAddressServiceClient addressServiceClient,
            ICatalogServiceClient catalogServiceClient,
            IDeliveryFeeCalculator deliveryFeeCalculator,
            IIntegrationEventPublisher eventPublisher)
        {
            _sender = sender;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _cartServiceClient = cartServiceClient;
            _addressServiceClient = addressServiceClient;
            _catalogServiceClient = catalogServiceClient;
            _deliveryFeeCalculator = deliveryFeeCalculator;
            _eventPublisher = eventPublisher;
        }

        public async Task<Result<PlaceOrderResponse>> Handle(
            PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result.Failure<PlaceOrderResponse>(Error.New("Order.Unauthorized", "User is not authenticated."));

            var cart = await _cartServiceClient.GetCartAsync(request.CartId, userId.Value, cancellationToken);
            if (cart is null || cart.Items.Count == 0)
                return Result.Failure<PlaceOrderResponse>(Error.New("Cart.Empty", "Your cart is empty or could not be found."));

            var addressResult = await ResolveAddressAsync(request, userId.Value, cancellationToken);
            if (addressResult.IsFailure)
                return Result.Failure<PlaceOrderResponse>(addressResult.Error);

            var address = addressResult.Value;

            if (!address.IsServiceable || address.StoreId is null)
                return Result.Failure<PlaceOrderResponse>(Error.New(
                    "Order.NotServiceable", "This address is outside our current delivery coverage."));

            var itemsResult = await BuildOrderItemsAsync(cart, cancellationToken);
            if (itemsResult.IsFailure)
                return Result.Failure<PlaceOrderResponse>(itemsResult.Error);

            var (orderItems, subtotal) = itemsResult.Value;

            var order = await AssembleOrderAsync(request, userId.Value, address, orderItems, subtotal, cancellationToken);

            var orderRepository = _unitOfWork.Repository<Order>();
            await orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new OrderConfirmedEvent 
                { 
                    OrderId = order.Id, 
                    UserId = userId.Value,
                    PaymentMethod = request.PaymentMethod.ToString(),
                    OrderNumber = order.OrderNumber,
                    Total = order.Total,
                    UserEmail = _currentUserService.Email
                },
                cancellationToken);

            return Result.Success(new PlaceOrderResponse(
                order.Id, order.OrderNumber, order.Status, order.PaymentStatus,
                order.PaymentMethod, order.Subtotal, order.DeliveryFee, order.Total));
        }

    

        private async Task<Result<OrderAddressDetails>> ResolveAddressAsync(
            PlaceOrderCommand request, Guid userId, CancellationToken cancellationToken)
        {
            if (request.AddressId.HasValue)
                return await _addressServiceClient.GetAddressForOrderAsync(request.AddressId.Value, userId, cancellationToken);

            var payload = new NewAddressRequestPayload(
                request.NewAddress!.RecipientName,
                request.NewAddress.RecipientPhone,
                request.NewAddress.AddressLine,
                request.NewAddress.City,
                request.NewAddress.Area,
                request.NewAddress.Lat,
                request.NewAddress.Lng);

            return await _addressServiceClient.CreateAddressForOrderAsync(userId, payload, cancellationToken);
        }

      

        private async Task<Result<(List<OrderItem> Items, decimal Subtotal)>> BuildOrderItemsAsync(
            CartDetailsDto cart, CancellationToken cancellationToken)
        {
            var lookups = await Task.WhenAll(cart.Items.Select(async cartItem =>
            {
                var product = await _catalogServiceClient.GetProductDetailsAsync(cartItem.ProductId, cancellationToken);
                return (CartItem: cartItem, Product: product);
            }));

            var missing = lookups.FirstOrDefault(l => l.Product is null);
            if (missing.CartItem is not null)
                return Result.Failure<(List<OrderItem>, decimal)>(Error.New(
                    "Product.NotFound", $"Product {missing.CartItem.ProductId} is no longer available."));

            var orderItems = lookups.Select(l => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = l.CartItem.ProductId,
                ProductName = l.Product!.Name,
                ProductImageUrl = l.Product.ImageUrl,
                UnitPrice = l.Product.Price,
                Quantity = l.CartItem.Quantity
            }).ToList();

            var subtotal = orderItems.Sum(i => i.UnitPrice * i.Quantity);

            return Result.Success((orderItems, subtotal));
        }

     
        private async Task<Order> AssembleOrderAsync(
            PlaceOrderCommand request,
            Guid userId,
            OrderAddressDetails address,
            List<OrderItem> orderItems,
            decimal subtotal,
            CancellationToken cancellationToken)
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
                Subtotal = subtotal
            };

            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                order.Items.Add(item);
            }

            order.DeliveryFee = await _deliveryFeeCalculator.CalculateAsync(address.StoreId.Value, address, cancellationToken);
            order.Total = order.Subtotal + order.DeliveryFee;

            if (request.IsGift)
            {
                order.IsGift = true;
                order.GiftRecipientName = request.GiftRecipient!.RecipientName;
                order.GiftRecipientPhone = request.GiftRecipient.RecipientPhone;
                order.GiftRecipientAddress = request.GiftRecipient.RecipientAddress;
            }

            order.AddressSnapshot = new OrderAddressSnapshot
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                RecipientName = address.RecipientName,
                RecipientPhone = address.RecipientPhone,
                AddressLine = address.AddressLine,
                City = address.City,
                Area = address.Area,
                Lat = address.Lat,
                Lng = address.Lng
            };

            return order;
        }

        private static string GenerateOrderNumber() =>
            $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
    }
}