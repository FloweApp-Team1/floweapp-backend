using FluentValidation;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrderDetails
{
    public class GetAssignedOrderDetailsValidator : AbstractValidator<GetAssignedOrderDetailsQuery>
    {
        public GetAssignedOrderDetailsValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
