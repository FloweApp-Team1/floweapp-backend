using MediatR;
using Shared.Results;
using System.Collections.Generic;

namespace AddressCartService.Features.Locations.GetGovernorates
{
    public record GetGovernoratesQuery : IRequest<Result<IReadOnlyList<GovernorateResponse>>>;
}
