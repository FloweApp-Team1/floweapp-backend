using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Shared.Results;

namespace CatalogService.Features.Admin.Products.ArchiveProduct
{
    public record ArchiveProductCommand(Guid ProductId) : IRequest<Result<ProductDto>>;
}
