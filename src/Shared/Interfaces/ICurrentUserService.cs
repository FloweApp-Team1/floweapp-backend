namespace IdentityService.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
        string? IpAddress { get; }
        string? DeviceName { get; }
    }
}
