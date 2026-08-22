using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Extensions;

namespace CatalogService.Features.Products.BatchGetProducts
{
    public class BatchGetProductsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/products/batch",
                    async (
                        [FromBody] BatchGetProductsRequest request,
                        ISender sender,
                        CancellationToken cancellationToken) =>
                    {
                        var result = await sender.Send(
                            new BatchGetProductsQuery(request.ProductIds, request.StoreId),
                            cancellationToken);

                        return result.ToMinimalApiResult("Batch products retrieved");
                    })
                .WithTags("Products")
                .WithName("BatchGetProducts")
                .AllowAnonymous();
        }
    }
}
