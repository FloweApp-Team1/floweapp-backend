using IdentityService.Common.Contracts;
using IdentityService.Common.Models;
using IdentityService.Common.Responses;
using IdentityService.Features.Drivers.Dtos_VM;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Drivers.ApplyAsDriver;

public class ApplyAsDriverEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/drivers/apply",async
            ([FromForm]ApplyDriverRequestCommand request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            if (!result.IsSuccess)
            {
                return  Results.BadRequest(new { error = result.Error });
            }
            return Results.Ok(
                ApiResponse<ApplyDriverDto>.Success(
                    result.Value,
                    "Driver application submitted successfully."));
        })
            .WithTags("Drivers")
            .WithName("ApplyAsDriver")
            .DisableAntiforgery() // multipart/form-data endpoint
            .AllowAnonymous();
    }
}
