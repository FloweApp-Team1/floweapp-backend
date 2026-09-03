using FluentValidation;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();

            // A bad status name fails to bind and never reaches here; this catches a numeric
            // body value that lands outside the three DriverStatusUpdate members.
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Status is not a status a driver can set.");

            // Matches the column width so an over-long note fails as bad input rather than
            // as a truncation error deep in the write.
            RuleFor(x => x.Note)
                .MaximumLength(500)
                .When(x => x.Note is not null);
        }
    }
}
