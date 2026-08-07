using System;
using System.Collections.Generic;

namespace IdentityService.Features.Auth.Login
{
    public record AuthResponse(UserDto User, string Token, string RefreshToken);

    public record UserDto(
        Guid Id,
        string Email,
        string Phone,
        string Name,
        List<string> Roles,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string Gender,
        string NotificationStatus
    );
}
