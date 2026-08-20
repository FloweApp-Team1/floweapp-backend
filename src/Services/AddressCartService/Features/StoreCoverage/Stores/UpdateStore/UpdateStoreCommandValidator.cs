using AddressCartService.Features.StoreCoverage.Common.Validation;
using FluentValidation;

namespace AddressCartService.Features.StoreCoverage.Stores.UpdateStore
{
    public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
    {
        public UpdateStoreCommandValidator()
        {
            RuleFor(x => x.StoreId).NotEmpty().WithMessage("storeId is required.");

            RuleFor(x => x.Request).NotNull().WithMessage("Request body is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.Name)
                    .NotEmpty().WithMessage("name is required.")
                    .MaximumLength(150).WithMessage("name must be 150 characters or fewer.");

                RuleFor(x => x.Request.Location)
                    .NotNull().WithMessage("location is required.");

                When(x => x.Request.Location is not null, () =>
                {
                    RuleFor(x => x.Request.Location.AddressLine)
                        .NotEmpty().WithMessage("location.addressLine is required.")
                        .MaximumLength(500).WithMessage("location.addressLine must be 500 characters or fewer.");

                    RuleFor(x => x.Request.Location.Lat)
                        .InclusiveBetween(-90, 90).WithMessage("location.lat must be between -90 and 90.");

                    RuleFor(x => x.Request.Location.Lng)
                        .InclusiveBetween(-180, 180).WithMessage("location.lng must be between -180 and 180.");
                });

                RuleFor(x => x.Request.CoverageArea)
                    .NotNull().WithMessage("coverageArea is required.");

                RuleFor(x => x)
                    .Custom((command, context) =>
                        CoverageAreaValidator.Validate(command.Request.CoverageArea, context))
                    .When(x => x.Request.CoverageArea is not null);
            });
        }
    }
}
