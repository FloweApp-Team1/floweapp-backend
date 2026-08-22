using FluentValidation;

namespace IdentityService.Features.Admin.CreateVehicles
{
    public class CreateVehicleValidator:AbstractValidator<CreateVehiclesCommand>
    {
        public CreateVehicleValidator()
        {
            RuleFor(c=>c.Name).NotEmpty().MaximumLength(25);
        }
    }
}
