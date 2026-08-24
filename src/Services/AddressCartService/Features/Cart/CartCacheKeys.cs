namespace AddressCartService.Features.Cart
{
    public static class CartCacheKeys
    {
        public static string Cart(Guid userId) => $"Cart:{userId}";
    }
}
