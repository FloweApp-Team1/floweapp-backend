using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AddressCartService.Infrastructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AddressCartDbContext _context;

        public LocationRepository(AddressCartDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Governorate>> GetGovernoratesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Governorates
                .OrderBy(g => g.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<City>> GetCitiesAsync(int governorateId, CancellationToken cancellationToken = default)
        {
            return await _context.Cities
                .Where(c => c.GovernorateId == governorateId)
                .OrderBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<City?> GetCityWithGovernorateAsync(int cityId, CancellationToken cancellationToken = default)
        {
            return await _context.Cities
                .Include(c => c.Governorate)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cityId, cancellationToken);
        }
    }
}
