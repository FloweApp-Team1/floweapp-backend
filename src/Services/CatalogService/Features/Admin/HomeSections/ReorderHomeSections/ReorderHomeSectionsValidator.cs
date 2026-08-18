using FluentValidation;

namespace CatalogService.Features.Admin.HomeSections.ReorderHomeSections
{
    public class ReorderHomeSectionsValidator : AbstractValidator<ReorderHomeSectionsCommand>
    {
        public ReorderHomeSectionsValidator()
        {
            RuleFor(x => x.Sections)
                .NotEmpty().WithMessage("At least one section is required.");

            RuleFor(x => x.Sections)
                .Must(sections => sections.Select(s => s.Id).Distinct().Count() == sections.Count)
                .WithMessage("Duplicate section ids are not allowed.")
                .When(x => x.Sections is { Count: > 0 });

            RuleForEach(x => x.Sections).ChildRules(section =>
            {
                section.RuleFor(s => s.Id).NotEmpty();
                section.RuleFor(s => s.Order).GreaterThanOrEqualTo(0).WithMessage("Order cannot be negative.");
            });
        }
    }
}
