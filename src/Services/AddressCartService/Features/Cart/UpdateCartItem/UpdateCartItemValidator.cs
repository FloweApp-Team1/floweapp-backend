using FluentValidation;

namespace AddressCartService.Features.Cart.UpdateCartItem
{
    public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemCommand>
    {
        public UpdateCartItemValidator()
        {
            RuleFor(x => x.ItemId)
                .NotEmpty()
                .WithMessage("ItemId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero. Use DELETE to remove items.");
        }
    }
}
