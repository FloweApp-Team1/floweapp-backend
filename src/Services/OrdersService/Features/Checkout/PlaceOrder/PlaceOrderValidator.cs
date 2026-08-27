using FluentValidation;
using OrdersService.Domain.Enums;
namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderValidator()
        {
            RuleFor(x => x.CartId).NotEmpty();
            RuleFor(x => x.AddressId).NotEmpty();
            RuleFor(x => x.IdempotencyKey).NotEmpty();
            RuleFor(x => x.PaymentMethod).IsInEnum();

            When(x => x.PaymentMethod == PaymentMethodEnum.Card, () =>
            {
                RuleFor(x => x.PaymentGateway)
                    .NotEmpty()
                    .WithMessage("paymentGateway is required when paymentMethod is Card.");
            });

            When(x => x.IsGift, () =>
            {
                RuleFor(x => x.GiftRecipient)
                    .NotNull()
                    .WithMessage("Gift recipient details are required when sending as a gift.");

                When(x => x.GiftRecipient is not null, () =>
                {
                    RuleFor(x => x.GiftRecipient!.RecipientName).NotEmpty().MaximumLength(150);

                    RuleFor(x => x.GiftRecipient!.RecipientPhone)
                        .NotEmpty()
                        .Matches(@"^01[0125][0-9]{8}$")
                        .WithMessage("Phone must be a valid Egyptian mobile number.");
                });
            });
        }
    }
}
