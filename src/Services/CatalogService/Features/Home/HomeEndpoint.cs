using CatalogService.Features.Home.Queries;
using MediatR;
using Shared.Contracts;
using Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CatalogService.Features.Home
{
    public class HomeEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/home/layout", async (Guid? storeId, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetHomeLayoutQuery(storeId));
                return result.ToMinimalApiResult("Home layout retrieved");
            })
            .WithTags("Home")
            .WithName("GetHomeLayout")
            .AllowAnonymous();
        }
    }
}
