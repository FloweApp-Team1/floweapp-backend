using MediatR;
using Shared.Interfaces;
using Shared.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AddressCartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AddressCartService.Features.Locations.GetCities
{
    public class GetCitiesHandler : IRequestHandler<GetCitiesQuery, Result<IReadOnlyList<CityResponse>>>
    {
        private readonly AddressCartService.Infrastructure.Repositories.ILocationRepository _repository;

        public GetCitiesHandler(AddressCartService.Infrastructure.Repositories.ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<CityResponse>>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetCitiesAsync(request.GovernorateId, cancellationToken);
            var cities = data.Select(c => new CityResponse(c.Id, c.GovernorateId, c.NameAr, c.NameEn)).ToList();

            return Result<IReadOnlyList<CityResponse>>.Success(cities);
        }
    }
}
