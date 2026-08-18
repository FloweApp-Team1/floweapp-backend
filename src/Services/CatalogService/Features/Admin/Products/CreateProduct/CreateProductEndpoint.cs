using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Security;
using System.Text.Json;

namespace CatalogService.Features.Admin.Products.CreateProduct;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/products", async (
                [FromForm] CreateProductRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
        {
            var storeStock = string.IsNullOrWhiteSpace(request.StoreStock)
                ? new List<StoreStockItem>()
                : JsonSerializer.Deserialize<List<StoreStockItem>>(
                      request.StoreStock, (JsonSerializerOptions?)null) ?? new List<StoreStockItem>();

            var command = new CreateProductCommand(
                request.Name,
                request.Description,
                request.Includes ?? new List<string>(),
                request.Price,
                request.DiscountPercent,
                request.CategoryIds,
                request.OccasionIds ?? new List<Guid>(),
                request.Images ?? new List<IFormFile>(),
                storeStock);

            var result = await sender.Send(command, cancellationToken);
            return result.ToMinimalApiResult("Product created");
        })
            .WithTags("Admin")
            .WithName("CreateProduct")
            .DisableAntiforgery()
            .RequireAuthorization(AppPolicies.AdminOnly);
    }
}

