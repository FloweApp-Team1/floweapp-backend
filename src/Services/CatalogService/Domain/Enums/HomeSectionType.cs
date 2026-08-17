namespace CatalogService.Domain.Enums
{
    // Section types for the server-driven home layout (HomeLayoutSection).
    // Values are persisted by the AddHomeLayoutSections migration - do not renumber.
    public enum HomeSectionType
    {
        Banner = 0,
        ProductRail = 1,
        OccasionRail = 2,
        CategoryRail = 3
    }
}
