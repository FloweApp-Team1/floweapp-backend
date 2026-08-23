using FluentValidation;

namespace AddressCartService.Features.Addresses.SetDefaultAddress
{
    public sealed class SetDefaultAddressValidator : AbstractValidator<SetDefaultAddressCommand>
    {
        public SetDefaultAddressValidator()
        {
            RuleFor(x => x.AddressId)
                .NotEmpty()
                .WithMessage("AddressId is required.");
        }
    }
}
