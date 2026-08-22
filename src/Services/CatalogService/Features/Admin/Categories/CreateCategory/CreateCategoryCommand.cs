using CatalogService.Features.Admin.Categories.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Results;

namespace CatalogService.Features.Admin.Categories.CreateCategory
{
    public record CreateCategoryCommand(string Name, int Order, IFormFile? Icon) : IRequest<Result<CategoryDto>>;
}
