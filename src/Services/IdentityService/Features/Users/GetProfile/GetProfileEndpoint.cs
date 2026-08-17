using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Users.GetProfile;

public class GetProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", async (
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProfileQuery(), cancellationToken);

            return result.ToMinimalApiResult("Profile retrieved");
        })
        .WithTags("Users")
        .WithName("GetProfile")
        .RequireAuthorization()
        .Produces<ApiResponse<GetProfileResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
