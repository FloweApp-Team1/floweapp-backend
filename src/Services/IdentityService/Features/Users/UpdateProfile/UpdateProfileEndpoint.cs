using FluentValidation;
using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileEndpoint : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/users/profile",                        // ← corrected route
            async Task<IResult> (
                UpdateProfileCommand request,
                [FromServices] IMediator mediator,
                [FromServices] IValidator<UpdateProfileCommand> validator, // ← injected
                CancellationToken cancellationToken) =>
            {
                var validationResult = await validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(ApiResponse<UpdateProfileResponse>.Fail(
                        "Validation failed",
                        400,
                        validationResult.Errors
                            .Select(e => new ApiError(e.PropertyName, e.ErrorMessage))
                            .ToList()));
                }

                var result = await mediator.Send(request, cancellationToken);

                if (!result.IsSuccess)
                {
                    if (result.Error == "Unauthorized")
                        return Results.Unauthorized();

                    return Results.BadRequest(
                        ApiResponse<UpdateProfileResponse>.Fail(result.Error!));
                }

                return Results.Ok(ApiResponse<UpdateProfileResponse>.Success(result.Value));
            })
            .WithTags("Users")
            .WithName("UpdateProfile")
            .RequireAuthorization();
    }
}