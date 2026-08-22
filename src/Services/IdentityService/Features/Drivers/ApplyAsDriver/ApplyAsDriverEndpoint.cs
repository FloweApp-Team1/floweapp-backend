using Shared.Contracts;
using Shared.Models;
using Shared.Results;
using Shared.Responses;
using IdentityService.Features.Drivers.Dtos_VM;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Features.Drivers.ApplyAsDriver;

public class ApplyAsDriverEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/drivers/apply",async
            ([FromForm] ApplyDriverRequestVM request,
            [FromServices]IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new ApplyDriverRequestCommand(
                         request.FirstName,
                         request.LastName,
                         request.Email,
                         request.Phone,
                         request.Gender,
                         request.VehiclePlateNumber,
                         request.VehicleTypeId,
                         request.VehicleCapacity,
                         request.LicenceImage,
                         request.Nid,
                         request.NidImage,
                         request.Password,
                         request.ConfirmPassword,
                         request.FcmToken
                         );

            var result = await mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                return  Results.BadRequest(ApiResponse<ApplyDriverResponseDto>.Fail(result.Error.Message));
            }
            return Results.Created(
               $"/admin/drivers/applications/{result.Value.Id}",
                ApiResponse<ApplyDriverResponseDto>.Success(
                    result.Value,
                    "Driver application submitted successfully."));
        })
            .WithTags("Drivers")
            .WithName("ApplyAsDriver")
            .DisableAntiforgery() // multipart/form-data endpoint
            .AllowAnonymous();
    }
}
