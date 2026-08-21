using OrdersService.Domain.Entities;
using Shared.Interfaces;
using Shared.Results;
using MediatR;

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
            
            // Includes are typically needed here, but assuming Repository has simple get for now.
            var order = await orderRepo.GetByIdAsync(request.OrderId);

            if (order == null || order.UserId != userId)
                return Result<GetOrderResponse>.Failure(Error.New("NotFound", "Order not found."));

            var response = new GetOrderResponse(
                order.Id,
                order.OrderNumber,
                order.Status.ToString(),
                order.PaymentMethod.ToString(),
                order.PaymentStatus.ToString(),
                order.Subtotal,
                order.DeliveryFee,
                order.Total,
                order.CreatedAt);

            return Result<GetOrderResponse>.Success(response);
        }
    }
}
