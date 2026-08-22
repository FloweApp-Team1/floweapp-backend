using MediatR;
using Shared.Results;

namespace AddressCartService.Features.StoreCoverage.ResolveStore
{
    // Exactly one of AddressId or (Lat & Lng) must be supplied - see ResolveStoreQueryValidator.
    public sealed record ResolveStoreQuery(
        Guid? AddressId,
        double? Lat,
        double? Lng) : IRequest<Result<ResolveStoreResponse>>;
}
