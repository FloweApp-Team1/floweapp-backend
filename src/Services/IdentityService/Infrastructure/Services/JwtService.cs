using IdentityService.Common.Security;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Settings;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, string? driverApplicationStatus = null)
    {
        var roleList = roles.ToList();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("fullName", user.FullName)
        };

        claims.AddRange(roleList.Select(role => new Claim(AppClaimTypes.Role, role)));

        // Only Driver tokens carry this — lets the DriverApproved policy check
        // status from the token itself, no DB hit needed on every request.
        if (roleList.Contains(AppRoles.Driver, StringComparer.OrdinalIgnoreCase)
            && driverApplicationStatus is not null)
        {
            claims.Add(new Claim(AppClaimTypes.ApplicationStatus, driverApplicationStatus));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshTokenValue()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    // Only the hash is ever persisted — a stolen DB row can't be replayed as a live session.
    public string HashRefreshTokenValue(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}