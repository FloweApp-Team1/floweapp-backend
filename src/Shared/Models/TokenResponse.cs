using System.Text.Json.Serialization;

namespace Shared.Models
{

    public record TokenResponse(
        string AccessToken,
        // Never serialized to the client — callers must set it as an HttpOnly
        // cookie and strip it from any JSON body, the same as RegisterCustomerResponse.
        [property: JsonIgnore] string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt);
}
