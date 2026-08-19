using MediatR;
using Shared.Results;

namespace OrdersService.Features.DriverDelivery.UpdateDriverLocation
{
    public record UpdateDriverLocationCommand(
        double Lat,
        double Lng,
        DateTime? RecordedAt) : IRequest<Result<UpdateDriverLocationResponse>>;
}
