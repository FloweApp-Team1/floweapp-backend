using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AddressCartService.Infrastructure.Repositories;
using MediatR;
using Shared.Results;

namespace AddressCartService.Features.Locations.GetCountries
{
    public class GetCountriesHandler : IRequestHandler<GetCountriesQuery, Result<IReadOnlyList<CountryResponse>>>
    {
        private readonly ILocationRepository _repository;

        public GetCountriesHandler(ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<CountryResponse>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetCountriesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                data = data.Where(c =>
                    c.NameEn.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.NameAr.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Equals(term, StringComparison.OrdinalIgnoreCase) ||
                    c.PhoneCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var countries = data
                .Select(c => new CountryResponse(c.Id, c.NameAr, c.NameEn, c.Code, c.PhoneCode))
                .ToList();

            return Result<IReadOnlyList<CountryResponse>>.Success(countries);
        }
    }
}
