using IdentityService.Common.Models;
using MediatR;

namespace IdentityService.Features.Users.GuestUser
{
    public sealed record CreateGuestCommand(
     string UserName
 ) : IRequest<Result<CreateGuestResponse>>;
}
