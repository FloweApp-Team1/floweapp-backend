using FluentValidation;

namespace OrdersService.Features.Orders.ConfirmDelivery;

public sealed class ConfirmDeliveryValidator : AbstractValidator<ConfirmDeliveryCommand>
{
    public ConfirmDeliveryValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
