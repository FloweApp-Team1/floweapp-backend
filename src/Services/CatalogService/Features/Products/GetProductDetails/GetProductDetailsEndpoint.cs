using MediatR;
using Shared.Contracts;
using Shared.Extensions;

namespace CatalogService.Features.Products.GetProductDetails;

public class GetProductDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/products/{id:guid}",
                async (
                    Guid id,
                    Guid? storeId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetProductDetailsQuery(id, storeId),
                        cancellationToken);

                    return result.ToMinimalApiResult("Product retrieved");
                })
            .WithTags("Products")
            .WithName("GetProductDetails")
            .AllowAnonymous();
    }
}