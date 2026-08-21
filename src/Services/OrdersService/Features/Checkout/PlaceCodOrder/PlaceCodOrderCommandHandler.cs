using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;
using Shared.Events.OrderEvents;
using Shared.Interfaces;
using Shared.Results;
using MediatR;


namespace OrdersService.Features.Checkout.PlaceCodOrder
{
    public class PlaceCodOrderCommandHandler : IRequestHandler<PlaceCodOrderCommand, Result<PlaceCodOrderResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIntegrationEventPublisher _publishEndpoint;

        public PlaceCodOrderCommandHandler(
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogServiceClient,
            ICurrentUserService currentUserService,
            IIntegrationEventPublisher publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _catalogServiceClient = catalogServiceClient;
            _currentUserService = currentUserService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<PlaceCodOrderResponse>> Handle(PlaceCodOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.Items == null || !request.Items.Any())
                return Result<PlaceCodOrderResponse>.Failure(Error.New("Validation", "Order must contain at least one item."));

            if (request.Items.Any(i => i.Quantity <= 0))
                return Result<PlaceCodOrderResponse>.Failure(Error.New("Validation", "Quantity must be greater than zero."));

            var userId = _currentUserService.UserId ?? Guid.Empty;
            var orderRepo = _unitOfWork.Repository<Order>();

            // Double submission guard: Only block if the user is currently in an active Stripe (Card) checkout flow.
            // COD orders stay 'Pending' until delivery, so they should not block future purchases.
            var existingPendingOrder = await orderRepo.FirstOrDefaultAsync(o => 
                o.UserId == userId && 
                o.PaymentStatus == PaymentStatusEnum.Pending &&
                o.PaymentMethod == PaymentMethodEnum.Card);
            if (existingPendingOrder != null)
            {
                return Result<PlaceCodOrderResponse>.Failure(Error.New("Conflict", "You already have a pending order. Please complete or cancel it first."));
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                UserId = userId,
                StoreId = request.StoreId,
                AddressId = request.AddressId,
                IsGift = request.IsGift,
                GiftRecipientName = request.GiftRecipientName,
                GiftRecipientPhone = request.GiftRecipientPhone,
                GiftRecipientAddress = request.GiftRecipientAddress,
                PaymentMethod = PaymentMethodEnum.Cod,
                Status = OrderStatusEnum.Placed,
                PaymentStatus = PaymentStatusEnum.Pending
            };

            decimal calculatedSubtotal = 0;

            foreach (var item in request.Items)
            {
                var catalogProduct = await _catalogServiceClient.GetProductDetailsAsync(item.ProductId, cancellationToken);
                if (catalogProduct == null)
                    return Result<PlaceCodOrderResponse>.Failure(Error.New("NotFound", $"Product with ID {item.ProductId} not found."));
                
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    ProductName = catalogProduct.Name,
                    ProductImageUrl = catalogProduct.ImageUrl,
                    UnitPrice = catalogProduct.Price, // Securely taken from CatalogService
                    Quantity = item.Quantity,
                    OrderId = order.Id
                };

                calculatedSubtotal += orderItem.UnitPrice * orderItem.Quantity;
                order.Items.Add(orderItem);
            }

            order.Subtotal = calculatedSubtotal;
            order.DeliveryFee = 5.0m;
            order.Total = order.Subtotal + order.DeliveryFee;

            await orderRepo.AddAsync(order);
            
            // Publish OrderPlacedEvent so Cart can be cleared
            await _publishEndpoint.PublishAsync(new OrderPlacedEvent { OrderId = order.Id, UserId = userId }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<PlaceCodOrderResponse>.Success(new PlaceCodOrderResponse(order.Id));
        }
    }
}
