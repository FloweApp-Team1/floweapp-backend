using Shared.Contracts;
using Shared.Extensions;
using Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
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
                [FromForm] UpdateProfileVM request,
                [FromServices] ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateProfileCommand(
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber,
                    request.Gender,
                    request.ProfilePicture);

                var result = await sender.Send(command, cancellationToken);

                return result.ToMinimalApiResult("Profile updated");
            })
            .WithTags("Users")
            .WithName("UpdateProfile")
            .RequireAuthorization()
            .DisableAntiforgery() // multipart/form-data endpoint
            .Produces<ApiResponse<UpdateProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}
