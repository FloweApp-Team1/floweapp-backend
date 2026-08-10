namespace IdentityService.Features.Admin.AdminLogin.Dtos
{
    public sealed record UserProfileDto(Guid Id, string FirstName, string LastName, string Email, IReadOnlyList<string> Roles);

}
