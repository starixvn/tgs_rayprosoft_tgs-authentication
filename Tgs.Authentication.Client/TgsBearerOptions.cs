using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client;

public class TgsBearerOptions : AuthenticationSchemeOptions
{
	private readonly JsonWebTokenHandler _defaultTokenHandler = new()
	{
		MapInboundClaims = JwtSecurityTokenHandler.DefaultMapInboundClaims
	};

	public IConfigurationManager<TokenConfiguration>? ConfigurationManager { get; set; }

	public TokenValidationParameters TokenValidationParameters { get; set; } = new()
	{
		NameClaimType = ClaimTypes.NameIdentifier
	};

	public IList<TokenHandler> TokenHandlers { get; private set; }
	public bool RefreshOnIssuerKeyNotFound { get; set; } = true;

	public TgsBearerOptions()
	{
		TokenHandlers = [_defaultTokenHandler];
	}
}