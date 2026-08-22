using FluentValidation;
using PaymentService.Features.Checkout.CreateCheckoutSession.Commands;

namespace PaymentService.Features.Checkout.CreateCheckoutSession.Validators
{
    public class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
    {
        public CreateCheckoutSessionCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required.");

            RuleFor(x => x.AmountTotal)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-character ISO code.");
        }
    }
}
