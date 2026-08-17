using CatalogService.Features.Admin.Occasions.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Results;

namespace CatalogService.Features.Admin.Occasions.UpdateOccasion
{
    public record UpdateOccasionCommand(
        Guid OccasionId,
        string? Name,
        int? Order,
        IFormFile? Image) : IRequest<Result<OccasionDto>>;
}
