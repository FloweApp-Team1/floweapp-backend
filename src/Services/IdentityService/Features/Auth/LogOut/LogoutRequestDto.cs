namespace IdentityService.Features.Auth.LogOut
{
   
        // DeviceId is expected for a device-scoped logout. Omitting it deliberately
        // falls back to logging the user out from every session and registered device.
        public sealed record LogoutRequestDto(string RefreshToken, string DeviceId);
    
}
