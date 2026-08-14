using CatalogService.Features.Admin.HomeSections.Common;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Admin.HomeSections.ReorderHomeSections
{
    public record SectionOrder(Guid Id, int Order);

    public record ReorderHomeSectionsCommand(List<SectionOrder> Sections) : IRequest<Result<List<HomeSectionDto>>>;
}
