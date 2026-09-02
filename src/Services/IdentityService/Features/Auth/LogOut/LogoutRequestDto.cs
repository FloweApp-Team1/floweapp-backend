namespace IdentityService.Features.Auth.LogOut
{
   
        public sealed record LogoutRequestDto(string RefreshToken, string? DeviceId);
    
}
