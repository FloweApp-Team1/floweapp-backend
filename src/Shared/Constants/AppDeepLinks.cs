namespace Shared.Constants
{
    public static class AppDeepLinks
    {
        public const string Scheme = "flowerapp://";

        public const string Categories = Scheme + "categories";
        public const string Products = Scheme + "products";
        public const string BestSellers = Scheme + "products?sort=best_sellers";
        public const string Occasions = Scheme + "occasions";
        
        public static string Promotions(string id) => Scheme + $"promotions?id={id}";
    }
}
