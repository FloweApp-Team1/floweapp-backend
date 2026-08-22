namespace AddressCartService.Infrastructure.Services.StoreCoverage
{
    public interface IStoreResolutionService
    {
        // Returns the Id of the first active store whose coverage area contains the
        // given point/city-area, or null if none does (address is still saved, just
        // flagged unserviceable - see CreateAddressHandler).
        Task<Guid?> ResolveServingStoreAsync(
            double? lat, double? lng, string city, string area, CancellationToken cancellationToken);
    }
}
