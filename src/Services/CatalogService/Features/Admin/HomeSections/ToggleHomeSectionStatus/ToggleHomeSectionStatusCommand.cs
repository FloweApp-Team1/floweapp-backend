using CatalogService.Features.Admin.HomeSections.Common;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Admin.HomeSections.ToggleHomeSectionStatus
{
    public record ToggleHomeSectionStatusCommand(Guid SectionId, bool Enabled) : IRequest<Result<HomeSectionDto>>;
}
