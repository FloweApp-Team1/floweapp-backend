using FluentValidation;
using Microsoft.Extensions.Options;
using OrdersService.Infrastructure.Settings;

namespace OrdersService.Features.DriverDelivery.UpdateDriverLocation
{
    public class UpdateDriverLocationValidator : AbstractValidator<UpdateDriverLocationCommand>
    {
        public UpdateDriverLocationValidator(IOptions<DeliveryTrackingSettings> settings)
        {
            var tracking = settings.Value;

            RuleFor(x => x.Lat)
                .InclusiveBetween(-90d, 90d)
                .WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Lng)
                .InclusiveBetween(-180d, 180d)
                .WithMessage("Longitude must be between -180 and 180.");

            // (0, 0) is in the Atlantic and is what a device reports when it has no fix yet -
            // accepting it would park the customer's map marker off the coast of Africa.
            RuleFor(x => x)
                .Must(x => Math.Abs(x.Lat) > 0.000001d || Math.Abs(x.Lng) > 0.000001d)
                .OverridePropertyName("Location")
                .WithMessage("A location of (0, 0) is not a valid fix.");

            RuleFor(x => x.RecordedAt!.Value)
                .GreaterThan(_ => DateTime.UtcNow.AddMinutes(-tracking.MaximumPingAgeMinutes))
                .WithMessage($"RecordedAt cannot be more than {tracking.MaximumPingAgeMinutes} minutes old.")
                .LessThan(_ => DateTime.UtcNow.AddSeconds(tracking.MaximumPingClockSkewSeconds))
                .WithMessage("RecordedAt cannot be in the future.")
                .OverridePropertyName(nameof(UpdateDriverLocationCommand.RecordedAt))
                .When(x => x.RecordedAt.HasValue);
        }
    }
}
