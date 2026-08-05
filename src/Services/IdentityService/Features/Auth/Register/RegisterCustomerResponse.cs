namespace IdentityService.Features.Auth.Register
{
    public class RegisterCustomerResponse
    {
        public UserDto User { get; init; } = default!;
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }

    public class UserDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public List<string> Roles { get; init; } = [];
        public DateTime CreatedAt { get; init; }
        public string Gender { get; init; } = string.Empty;
        public string NotificationStatus { get; init; } = string.Empty;
    }
}