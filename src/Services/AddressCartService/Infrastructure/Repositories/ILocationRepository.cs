using AddressCartService.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AddressCartService.Infrastructure.Repositories
{
    public interface ILocationRepository
    {
        Task<IReadOnlyList<Governorate>> GetGovernoratesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<City>> GetCitiesAsync(int governorateId, CancellationToken cancellationToken = default);
        Task<City?> GetCityWithGovernorateAsync(int cityId, CancellationToken cancellationToken = default);
    }
}
