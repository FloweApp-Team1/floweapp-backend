using MediatR;
using Shared.Responses;
using Shared.Results;

namespace CatalogService.Features.Products.GetProductDetails
{
    public sealed record GetProductDetailsQuery(Guid ProductId, Guid? StoreId) : IRequest<Result<GetProductDetailsResponse>>;
    
    

}
