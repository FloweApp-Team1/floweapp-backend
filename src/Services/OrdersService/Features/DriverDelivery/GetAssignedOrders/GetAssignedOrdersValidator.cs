using FluentValidation;

namespace OrdersService.Features.DriverDelivery.GetAssignedOrders
{
    public class GetAssignedOrdersValidator : AbstractValidator<GetAssignedOrdersQuery>
    {
        public GetAssignedOrdersValidator()
        {
            RuleFor(x => x.Request.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be greater than or equal to 1.");

            RuleFor(x => x.Request.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page size must be greater than or equal to 1.")
                .LessThanOrEqualTo(50)
                .WithMessage("Page size cannot exceed 50.");

            // Query-string binding accepts any integer as an enum, so an out-of-range
            // ?status=99 would otherwise reach the handler as an undefined value. A null
            // (filter omitted) is fine.
            RuleFor(x => x.Status)
                .Must(status => status is null || Enum.IsDefined(status.Value))
                .WithMessage("Status is not a known order status.");
        }
    }
}
