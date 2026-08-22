using FluentValidation;

namespace OrdersService.Features.Tracking.GetOrderTracking
{
    public class GetOrderTrackingValidator : AbstractValidator<GetOrderTrackingQuery>
    {
        public GetOrderTrackingValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
