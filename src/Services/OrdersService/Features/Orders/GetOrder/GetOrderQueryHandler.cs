using OrdersService.Domain.Entities;
using Shared.Interfaces;
using Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Enums;

namespace OrdersService.Features.Orders.GetOrder
{
    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, Result<GetOrderResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<GetOrderResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var orderRepo = _unitOfWork.Repository<Order>();
            
            var orderData = await orderRepo.Query()
                .Where(o => o.Id == request.OrderId)
                .Select(o => new
                {
                    o.Id,
                    o.UserId,
                    o.OrderNumber,
                    o.Status,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.Subtotal,
                    o.DeliveryFee,
                    o.Total,
                    o.CreatedAt,
                    o.IsGift,
                    o.GiftRecipientName,
                    o.GiftRecipientPhone,
                    o.GiftRecipientAddress,
                    Items = o.Items.Select(i => new OrderItemDto(
                        i.ProductId,
                        i.ProductName,
                        i.ProductImageUrl,
                        i.UnitPrice,
                        i.Quantity
                    )).ToList(),
                    Address = o.AddressSnapshot != null ? new OrderAddressDto(
                        o.AddressSnapshot.City,
                        o.AddressSnapshot.Area,
                        o.AddressSnapshot.AddressLine,
                        o.AddressSnapshot.RecipientName,
                        o.AddressSnapshot.RecipientPhone,
                        o.AddressSnapshot.Lat,
                        o.AddressSnapshot.Lng
                    ) : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (orderData == null || orderData.UserId != userId)
                return Result<GetOrderResponse>.Failure(Error.New("NotFound", "Order not found."));

            var response = new GetOrderResponse(
                orderData.Id,
                orderData.OrderNumber,
                orderData.Status.ToDisplayString(),
                orderData.PaymentMethod.ToString(),
                orderData.PaymentStatus.ToString(),
                orderData.Subtotal,
                orderData.DeliveryFee,
                orderData.Total,
                orderData.CreatedAt,
                orderData.Items,
                orderData.Address,
                orderData.IsGift,
                orderData.GiftRecipientName,
                orderData.GiftRecipientPhone,
                orderData.GiftRecipientAddress,
                orderData.Status.IsActiveDelivery());

            return Result<GetOrderResponse>.Success(response);
        }
    }
}
