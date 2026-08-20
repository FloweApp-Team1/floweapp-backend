namespace CatalogService.Domain.Enums
{
    // Section kinds for the admin-authored HomeSection model.
    // Distinct from HomeSectionType, which belongs to the server-driven
    // HomeLayoutSection read model: a "Rail" here carries its content through the
    // SectionCategories/SectionOccasions/SectionProducts collections plus
    // ProductSelectionRule, rather than encoding the content type in the enum.
    public enum HomeSectionKind
    {
        Banner = 1,
        Rail = 2
    }
}
