using MediatR;
using IdentityService.Common.Contracts;
using IdentityService.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Users.UpdateProfile;

public class UpdateProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/users/UpdateProfile",
            async Task<ApiResponse<UpdateProfileResponse>> (
                UpdateProfileCommand request,
                [FromServices]IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var validator = new UpdateProfileValidator();

                var validationResult = await validator
                    .ValidateAsync(request, cancellationToken);


                if (!validationResult.IsValid)
                {
                    return ApiResponse<UpdateProfileResponse>.Fail(
                        "Validation failed",
                        400,
                        validationResult.Errors
                            .Select(e => new ApiError(
                                e.PropertyName,
                                e.ErrorMessage))
                            .ToList());
                }


                var result = await mediator.Send(
                    request,
                    cancellationToken);


                if (!result.IsSuccess)
                {
                    return ApiResponse<UpdateProfileResponse>.Fail(
                        result.Error!);
                }


                return ApiResponse<UpdateProfileResponse>.Success(
                    result.Value);

            })
            .WithTags("Users")
            .WithName("UpdateProfile")
            .RequireAuthorization();
    }
}