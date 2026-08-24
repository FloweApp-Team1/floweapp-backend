using MediatR;
using Shared.Results;
using System.Collections.Generic;

namespace AddressCartService.Features.Locations.GetCities
{
    public record GetCitiesQuery(int GovernorateId) : IRequest<Result<IReadOnlyList<CityResponse>>>;
}
