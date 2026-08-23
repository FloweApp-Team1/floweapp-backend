using FluentValidation;

namespace OrdersService.Features.Checkout.PlaceOrder
{
    
        public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
        {
            public PlaceOrderValidator()
            {
                RuleFor(x => x.CartId).NotEmpty();

                RuleFor(x => x)
                    .Must(x => x.AddressId.HasValue ^ x.NewAddress is not null)
                    .WithMessage("Provide either addressId or newAddress, not both or neither.");

                RuleFor(x => x.PaymentMethod).IsInEnum();

                When(x => x.NewAddress is not null, () =>
                {
                    RuleFor(x => x.NewAddress!.RecipientName).NotEmpty().MaximumLength(150);

                    RuleFor(x => x.NewAddress!.RecipientPhone)
                        .NotEmpty()
                        .Matches(@"^01[0125][0-9]{8}$")
                        .WithMessage("Phone must be a valid Egyptian mobile number.");

                    RuleFor(x => x.NewAddress!.AddressLine).NotEmpty().MaximumLength(500);
                    RuleFor(x => x.NewAddress!.City).NotEmpty().MaximumLength(100);
                    RuleFor(x => x.NewAddress!.Area).NotEmpty().MaximumLength(100);
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

                        RuleFor(x => x.GiftRecipient!.RecipientAddress).NotEmpty().MaximumLength(500);
                    });
                });
            }
        }
}
