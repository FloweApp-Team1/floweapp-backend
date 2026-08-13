using Shared.Contracts;
using Shared.Responses;
using MediatR;

namespace IdentityService.Features.Users.GuestUser
{
    public class CreateGuestEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/guests",
            async (
                CreateGuestCommand request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    request,
                    cancellationToken);

                return ApiResponse.Success(result);
            })
            .WithTags("Guest")
            .WithName("CreateGuest")
            .AllowAnonymous();
        }
    }
}
