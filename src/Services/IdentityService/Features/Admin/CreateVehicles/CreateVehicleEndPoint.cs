using IdentityService.Features.Admin.CreateVehicles.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Responses;
using Shared.Security;

namespace IdentityService.Features.Admin.CreateVehicles
{
    public class CreateVehicleEndPoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/admin/vehicles/Create", async (string Name,CancellationToken cancellationToken,[FromServices]IMediator mediator ) =>
            {

                var result = await mediator.Send(new CreateVehiclesCommand(Name), cancellationToken);
                if (result.IsSuccess)
                {
                    var responseVM = new CreateVehicleResponseVM
                    {
                        Id = result.Value.Id.ToString(),
                        Name = result.Value.Name,
                        CreatedAt= result.Value.CreatedAt,
                        CreatedBy=result.Value.CreatedBy.ToString()
                    };
                    return Results.Ok(ApiResponse<CreateVehicleResponseVM>.Success(responseVM));
                }

                return Results.BadRequest(ApiResponse<CreateVehicleResponseVM>.Fail(result.Error.Message));


            })
             .WithTags("Admin")
            .WithName("CreateVehicle")
            .RequireAuthorization(AppPolicies.AdminOnly);
        }
    }
}
