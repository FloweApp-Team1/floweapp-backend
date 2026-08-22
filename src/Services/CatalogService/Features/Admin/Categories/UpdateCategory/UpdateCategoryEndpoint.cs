using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;

namespace CatalogService.Features.Admin.Categories.UpdateCategory;
public class UpdateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/categories/{categoryId:guid}", async (
                Guid categoryId,
                [FromForm] UpdateCategoryRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var command = new UpdateCategoryCommand(categoryId, request.Name, request.Order, request.Icon);
            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Category updated");
        })
            .WithTags("Admin")
            .WithName("UpdateCategory")
            .DisableAntiforgery()
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
