using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client;

public class TgsAuthenticateManager(
	IOptions<TgsBearerOptions> optionsAccessor,
	ILogger<TgsAuthenticateManager> logger)
{
	private TgsBearerOptions Options { get; } = optionsAccessor.Value;

	public async Task<AuthenticationTicket?> ValidateTokenAsync(
		string token,
		string scheme,
		IList<Exception> validationFailures,
		CancellationToken cancellationToken = default)
	{
		var tvp = await SetupTokenValidationParametersAsync(
			cancellationToken).ConfigureAwait(false);
		SecurityToken validatedToken = null!;
		ClaimsPrincipal principal = null!;

		foreach (TokenHandler tokenHandler in Options.TokenHandlers)
		{
			try
			{
				var tokenValidationResult = await tokenHandler.ValidateTokenAsync(
					token,
					tvp).ConfigureAwait(false);
				if (tokenValidationResult.IsValid)
				{
					principal = new ClaimsPrincipal(tokenValidationResult.ClaimsIdentity);
					validatedToken = tokenValidationResult.SecurityToken;
					break;
				}

				RecordTokenValidationError(
					tokenValidationResult.Exception ?? new SecurityTokenValidationException(
						$"The TokenHandler: '{tokenHandler}', was unable to validate the Token."),
					validationFailures);
				continue;
			}
			catch (Exception exception)
			{
				validationFailures ??= new List<Exception>(1);

				RecordTokenValidationError(exception, validationFailures);
				continue;
			}
		}

		if (principal is not null && validatedToken is not null)
		{
			logger.TokenValidationSucceeded();

			var properties = new AuthenticationProperties
			{
				ExpiresUtc = GetSafeDateTime(validatedToken.ValidTo),
				IssuedUtc = GetSafeDateTime(validatedToken.ValidFrom)
			};

			properties.StoreTokens([
				new AuthenticationToken
				{
					Name = "access_token",
					Value = token
				}
			]);

			return new AuthenticationTicket(principal, properties, scheme);
		}

		return null;
	}

	private static DateTime? GetSafeDateTime(DateTime dateTime)
		=> dateTime == DateTime.MinValue ? null : dateTime;

	private void RecordTokenValidationError(Exception exception, IList<Exception> exceptions)
	{
		if (exception != null)
		{
			logger.TokenValidationFailed(exception);
			exceptions.Add(exception);
		}

		if (Options.RefreshOnIssuerKeyNotFound && Options.ConfigurationManager != null && exception is SecurityTokenSignatureKeyNotFoundException)
			Options.ConfigurationManager.RequestRefresh();
	}

	private async Task<TokenValidationParameters> SetupTokenValidationParametersAsync(CancellationToken cancellationToken = default)
	{
		var tokenValidationParameters = Options.TokenValidationParameters.Clone();
		if (Options.ConfigurationManager is BaseConfigurationManager configurationManager)
			tokenValidationParameters.ConfigurationManager = configurationManager;

		if (Options.ConfigurationManager is not null)
		{
			var tokenConfiguration = await Options.ConfigurationManager.GetConfigurationAsync(
				cancellationToken).ConfigureAwait(false);

			if (!string.IsNullOrEmpty(tokenConfiguration.Issuer))
			{
				string[] array = [tokenConfiguration.Issuer];
				IEnumerable<string> validIssuers;
				if (tokenValidationParameters.ValidIssuers != null)
				{
					validIssuers = tokenValidationParameters.ValidIssuers.Concat(array);
				}
				else
				{
					IEnumerable<string> enumerable = array;
					validIssuers = enumerable;
				}

				tokenValidationParameters.ValidIssuers = validIssuers;
			}

			var jwks = new JsonWebKeySet();
			jwks.Keys.Add(tokenConfiguration.JsonWebKey);

			tokenValidationParameters.IssuerSigningKeys = tokenValidationParameters.IssuerSigningKeys is not null
				? tokenValidationParameters.IssuerSigningKeys.Concat(jwks.GetSigningKeys())
				: (IEnumerable<SecurityKey>)jwks.GetSigningKeys();
		}

		return tokenValidationParameters;
	}
}