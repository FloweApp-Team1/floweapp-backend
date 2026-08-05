using IdentityService.Common.Models;
using IdentityService.Features.Drivers.Dtos_VM;
using MediatR;

namespace IdentityService.Features.Drivers.ApplyAsDriver
{
    public class ApplyDriverRequestCommandHandler : IRequestHandler<ApplyDriverRequestCommand, Result<ApplyDriverDto>>
    {
        public Task<Result<ApplyDriverDto>> Handle(ApplyDriverRequestCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
