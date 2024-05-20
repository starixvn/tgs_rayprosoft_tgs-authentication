using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client.UnitTests;

internal static class TestsHelpers
{
    private static readonly JwtSecurityTokenHandler _JwtSecurityTokenHandler = new();

    public static string CreateJwtTokenString(ClaimsIdentity identity, SigningCredentials signingCredentials, TimeProvider timeProvider)
    {
        var system = identity.FindFirst(TgsClaimTypes.System)?.Value;
        var currentTime = timeProvider.GetUtcNow();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = "https://localhost",
            Audience = system ?? string.Empty,
            Subject = identity,
            Expires = currentTime.LocalDateTime.AddMinutes(5D),
            IssuedAt = currentTime.LocalDateTime,
            SigningCredentials = signingCredentials
        };

        var token = _JwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        return _JwtSecurityTokenHandler.WriteToken(token);
    }
}