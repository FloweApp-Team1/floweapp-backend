namespace Shared.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        Guid? SessionId => null;
        string? Email { get; }
        string? IpAddress { get; }
        string? DeviceName { get; }
    }
}
