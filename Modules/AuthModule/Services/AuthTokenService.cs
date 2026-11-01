using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetSkeleton.Modules.AuthModule.Configuration;
using DotNetSkeleton.Modules.AuthModule.Constants;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotNetSkeleton.Modules.AuthModule.Services;

public sealed class AuthTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(UserEntity user, AuthSessionEntity session)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.PhoneNumber, user.Phone),
            new("public_id", user.PublicId.ToString()),
            new("session_id", session.Id.ToString()),
            new(ClaimTypes.Role, user.Role switch
            {
                UserRole.User => "USER",
                UserRole.Admin => "ADMIN",
                _ => throw new InvalidOperationException($"Unsupported user role: {user.Role}.")
            })
        };

        // Permission profiles contain the fine-grained grants for ADMIN users only.
        if (user.Role == UserRole.Admin && user.Permission is { Status: PermissionStatus.Active } permission)
        {
            foreach (var detail in permission.Details)
            {
                claims.Add(new Claim(
                    PermissionConstants.PermissionClaim,
                    $"{detail.Function.NameEn.ToLowerInvariant()}:{detail.Action.ToLowerInvariant()}"));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt);
    }
}
