namespace IdentityService.Features.Admin.AdminLogin.Dtos
{
    public sealed record UserProfileDto(Guid Id, string FullName, string Email, IReadOnlyList<string> Roles);

}
