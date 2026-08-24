using MediatR;
using Shared.Interfaces;
using Shared.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AddressCartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AddressCartService.Features.Locations.GetGovernorates
{
    public class GetGovernoratesHandler : IRequestHandler<GetGovernoratesQuery, Result<IReadOnlyList<GovernorateResponse>>>
    {
        private readonly AddressCartService.Infrastructure.Repositories.ILocationRepository _repository;

        public GetGovernoratesHandler(AddressCartService.Infrastructure.Repositories.ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<GovernorateResponse>>> Handle(GetGovernoratesQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetGovernoratesAsync(cancellationToken);
            var governorates = data.Select(g => new GovernorateResponse(g.Id, g.NameAr, g.NameEn)).ToList();

            return Result<IReadOnlyList<GovernorateResponse>>.Success(governorates);
        }
    }
}
