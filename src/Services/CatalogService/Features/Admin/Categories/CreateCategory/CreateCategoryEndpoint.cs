using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;
namespace CatalogService.Features.Admin.Categories.CreateCategory;
public class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/categories", async (
                [FromForm] CreateCategoryRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var command = new CreateCategoryCommand(request.Name, request.Order, request.Icon);
            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Category created");
        })
            .WithTags("Admin")
            .WithName("CreateCategory")
            .DisableAntiforgery()
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
