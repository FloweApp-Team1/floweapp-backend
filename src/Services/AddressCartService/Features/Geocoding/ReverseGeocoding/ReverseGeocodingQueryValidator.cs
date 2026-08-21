using FluentValidation;

namespace AddressCartService.Features.Geocoding.ReverseGeocoding
{
    public class ReverseGeocodingQueryValidator : AbstractValidator<ReverseGeocodingQuery>
    {
        public ReverseGeocodingQueryValidator()
        {
            RuleFor(x => x.Lat)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Lng)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
        }
    }
}
