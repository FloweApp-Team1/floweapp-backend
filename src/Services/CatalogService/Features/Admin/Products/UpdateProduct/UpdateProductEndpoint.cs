using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;
using System.Text.Json;

namespace CatalogService.Features.Admin.Products.UpdateProduct;
public class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/products/{productId:guid}", async (
                Guid productId,
                [FromForm] UpdateProductRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            List<StoreStockItem>? storeStock = null;
            if (!string.IsNullOrWhiteSpace(request.StoreStock))
                storeStock = JsonSerializer.Deserialize<List<StoreStockItem>>(
                    request.StoreStock, (JsonSerializerOptions?)null);

            var command = new UpdateProductCommand(
                productId,
                request.Name,
                request.Description,
                request.Includes,
                request.Price,
                request.DiscountPercent,
                request.CategoryIds,
                request.OccasionIds,
                request.Images,
                storeStock);

            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Product updated");
        })
            .WithTags("Admin")
            .WithName("UpdateProduct")
            .DisableAntiforgery()
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}
