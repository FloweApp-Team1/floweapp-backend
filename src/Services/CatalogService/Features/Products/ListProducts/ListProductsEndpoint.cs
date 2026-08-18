using CatalogService.Features.Products.ListProducts.Dtos_VM;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Requests;
using Shared.Responses;

namespace CatalogService.Features.Products.ListProducts;

public class ListProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async ([AsParameters]PaginationRequest request,[FromQuery]Guid categoryId,CancellationToken cancellationToken,[FromServices]IMediator mediator) =>
        {
            var result= await mediator.Send(new ListProductsQuery(request,categoryId), cancellationToken);
            if(result.IsSuccess)
            {
                var responseVM=result.Value.Items.Select(x=>new ListProductResponseVM
                {
                    Id=x.Id.ToString(),
                    Name=x.Name,
                    DiscountPercentage=x.DiscountPercentage,
                    DiscountPrice=x.DiscountPrice,
                    IsOutOfStock=x.IsOutOfStock,
                    OrignalPrice=x.OrignalPrice,    
                    ProductImages=x.ProductImages.Select(pi=>new ProductImageVM
                    {
                        Id=pi.Id.ToString(),
                        ImageUrl=pi.ImageUrl,
                        DisplayOrder=pi.DisplayOrder,
                        IsPrimary=pi.IsPrimary
                    }).ToList()

                }).ToList();
               
                return Results.Ok(ApiResponse.Paginated<ListProductResponseVM>(responseVM,result.Value.TotalCount,request));
            }
            return Results.BadRequest(ApiResponse.Fail(result.Error.Message));
        })
            .WithTags("Products")
            .WithName("ListProducts")
            .AllowAnonymous();
    }
}
