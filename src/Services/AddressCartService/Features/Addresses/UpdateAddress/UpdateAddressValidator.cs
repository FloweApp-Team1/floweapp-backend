using FluentValidation;

namespace AddressCartService.Features.Addresses.UpdateAddress
{
    public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressValidator()
        {
            RuleFor(x => x.AddressId).NotEmpty();

            RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(150);

           
            RuleFor(x => x.RecipientPhone)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^01[0125][0-9]{8}$")
                .WithMessage("Phone must be a valid Egyptian mobile number.");

            RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(500);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Area).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Label).MaximumLength(50);

            RuleFor(x => x.Lat).InclusiveBetween(-90, 90).When(x => x.Lat.HasValue);
            RuleFor(x => x.Lng).InclusiveBetween(-180, 180).When(x => x.Lng.HasValue);
        }
    }
}
