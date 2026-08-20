using CatalogService.Features.Admin.Categories.Common;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Admin.Categories.ArchiveCategory
{
    public record ArchiveCategoryCommand(Guid CategoryId) : IRequest<Result<CategoryDto>>;
}
