using CatalogService.Features.Admin.Occasions.Common;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Admin.Occasions.ArchiveOccasion
{
    public record ArchiveOccasionCommand(Guid OccasionId) : IRequest<Result<OccasionDto>>;
}
