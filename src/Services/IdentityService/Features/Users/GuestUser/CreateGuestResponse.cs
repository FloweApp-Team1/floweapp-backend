namespace IdentityService.Features.Users.GuestUser
{
    public sealed record CreateGuestResponse(
     Guid Id,
     string UserName,
     DateTime CreatedAt
 );
}
