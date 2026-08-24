using FluentValidation;

namespace AddressCartService.Features.Addresses.CreateAddress
{
    public class CreateAddressValidator : AbstractValidator<CreateAddressCommand>
    {
        // Same pattern as registration: 01[0-2,5]XXXXXXXX, 11 digits total.
        private const string EgyptianPhonePattern = @"^01[0125][0-9]{8}$";

        public CreateAddressValidator()
        {
            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("Recipient name is required.")
                .MaximumLength(150);

            RuleFor(x => x.RecipientPhone)
                .NotEmpty().WithMessage("Recipient phone is required.")
                .Matches(EgyptianPhonePattern)
                .WithMessage("Phone number must be a valid Egyptian mobile number (e.g. 01xxxxxxxxx).");

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Address line is required.")
                .MaximumLength(500);

            RuleFor(x => x.GovernorateId)
                .GreaterThan(0).WithMessage("Governorate is required.");
                
            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("City is required.");

            RuleFor(x => x.Area)
                .NotEmpty().WithMessage("Area is required.")
                .MaximumLength(100);

            RuleFor(x => x.Label)
                .MaximumLength(50);

            RuleFor(x => x.Lat)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.")
                .When(x => x.Lat.HasValue);

            RuleFor(x => x.Lng)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.")
                .When(x => x.Lng.HasValue);
        }
    }
}
