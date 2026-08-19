using FluentValidation;

namespace OrdersService.Features.DriverDelivery.UpdateOrderStatus
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Status is not a known order status.");

            // Matches the column width so an over-long note fails as bad input rather than
            // as a truncation error deep in the write.
            RuleFor(x => x.Note)
                .MaximumLength(500)
                .When(x => x.Note is not null);
        }
    }
}
