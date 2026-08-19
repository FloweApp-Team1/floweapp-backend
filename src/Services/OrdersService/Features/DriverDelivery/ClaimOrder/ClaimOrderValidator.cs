using FluentValidation;

namespace OrdersService.Features.DriverDelivery.ClaimOrder
{
    public class ClaimOrderValidator : AbstractValidator<ClaimOrderCommand>
    {
        public ClaimOrderValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
