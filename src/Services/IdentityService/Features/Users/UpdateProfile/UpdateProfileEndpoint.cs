using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Validation is handled once, by the FluentValidation pipeline behavior;
        // the endpoint only translates the Result into the ApiResponse envelope.
        app.MapPut(
            "/users/profile",
            async Task<IResult> (
                UpdateProfileCommand request,
                [FromServices] ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(request, cancellationToken);

                return result.ToMinimalApiResult("Profile updated");
            })
            .WithTags("Users")
            .WithName("UpdateProfile")
            .RequireAuthorization()
            .Produces<ApiResponse<UpdateProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
