using FluentValidation;

namespace AddressCartService.Features.StoreCoverage.ResolveStore
{
    public class ResolveStoreQueryValidator : AbstractValidator<ResolveStoreQuery>
    {
        public ResolveStoreQueryValidator()
        {
            RuleFor(x => x)
                .Must(x => x.AddressId.HasValue || (x.Lat.HasValue && x.Lng.HasValue))
                .WithMessage("Provide either addressId, or both lat and lng.");

            RuleFor(x => x.Lat)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.")
                .When(x => x.Lat.HasValue);

            RuleFor(x => x.Lng)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.")
                .When(x => x.Lng.HasValue);
        }
    }
}
