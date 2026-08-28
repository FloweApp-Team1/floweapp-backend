using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Features.Checkout.PlaceOrder;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    
    public sealed record CreateOrderCommand(Order Order) : IRequest<Result<Guid>>;
  
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken) =>
            _unitOfWork.ExecuteAsync(async () =>
            {
                var orderRepository = _unitOfWork.Repository<Order>();
                await orderRepository.AddAsync(request.Order, cancellationToken);

                return Result.Success(request.Order.Id);
            }, cancellationToken);
    }
}
