namespace AddressCartService.Features.StoreCoverage.ResolveStore
{
    public sealed record ResolveStoreResponse(Guid? StoreId, bool IsServiceable);
}
