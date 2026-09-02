using FluentValidation;
using OrdersService.Domain.Enums;

namespace OrdersService.Features.Orders.AdminUpdateOrderStatus
{
    public class AdminUpdateOrderStatusValidator
    : AbstractValidator<AdminUpdateOrderStatusCommand>
    {
        public AdminUpdateOrderStatusValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.Status)
                .Equal(OrderStatusEnum.Preparing)
                .WithMessage("Admin can only move order to Preparing.");

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .When(x => x.Note != null);
        }
    }
}
