using CatalogService.Domain.Enums;

namespace CatalogService.Features.Admin.HomeSections.Common
{
    public static class ProductSelectionRuleParser
    {
        public static ProductSelectionRule Parse(string rule) => rule.ToUpperInvariant() switch
        {
            "MANUAL" => ProductSelectionRule.Manual,
            "BEST_SELLERS" => ProductSelectionRule.BestSellers,
            "NEW_ARRIVALS" => ProductSelectionRule.NewArrivals,
            "FEATURED" => ProductSelectionRule.Featured,
            "ON_SALE" => ProductSelectionRule.OnSale,
            _ => throw new ArgumentOutOfRangeException(nameof(rule), rule, "Unknown product selection rule.")
        };
    }
}
