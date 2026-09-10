using MediatR;
using Shared.Results;
using System.Collections.Generic;

namespace AddressCartService.Features.Locations.GetCountries
{
    public record GetCountriesQuery(string? Search = null) : IRequest<Result<IReadOnlyList<CountryResponse>>>;
}
