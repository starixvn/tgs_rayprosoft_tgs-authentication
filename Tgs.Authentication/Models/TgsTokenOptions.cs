using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Models;

public class TgsTokenOptions : AuthenticationSchemeOptions
{
    public string? Issuer { get; set; }
    public TimeSpan BearerTokenExpiration { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan RefreshTokenExpiration { get; set; } = TimeSpan.FromDays(14);
    public SigningCredentials SigningCredentials { get; set; } = default!;
}