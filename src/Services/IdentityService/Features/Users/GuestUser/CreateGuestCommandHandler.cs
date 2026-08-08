using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Users.GuestUser;

public class CreateGuestCommandHandler(
    IGenericRepository<Guest> _guestRepository,
    IUnitOfWork _unitOfWork,
    IHttpContextAccessor _httpContextAccessor)
    : IRequestHandler<CreateGuestCommand, Result<CreateGuestResponse>>
{
    public async Task<Result<CreateGuestResponse>> Handle(
        CreateGuestCommand request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            IpAddress = httpContext?
         .Connection?
         .RemoteIpAddress?
         .ToString(),

            DeviceInfo = httpContext?
         .Request?
         .Headers["User-Agent"]
         .ToString()
        };


        await _guestRepository.AddAsync(
            guest,
            cancellationToken);


        await _unitOfWork.SaveChangesAsync(
            cancellationToken);


        var response = new CreateGuestResponse(
            guest.Id,
            guest.UserName,
            guest.CreatedAt);


        return Result<CreateGuestResponse>
            .Success(response);
    }
}