namespace Shared.Models
{
    public record SessionDto(
        Guid Id,
        string? DeviceName,
        string? IpAddress,
        string? Location,
        DateTime CreatedAt,
        DateTime? LastUsedAt,
        bool IsCurrent);
}

