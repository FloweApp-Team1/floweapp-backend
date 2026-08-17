using FluentValidation;

namespace CatalogService.Features.Admin.HomeSections.ToggleHomeSectionStatus
{
    public class ToggleHomeSectionStatusValidator : AbstractValidator<ToggleHomeSectionStatusCommand>
    {
        public ToggleHomeSectionStatusValidator()
        {
            RuleFor(x => x.SectionId).NotEmpty();
        }
    }
}
