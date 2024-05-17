using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client;

public class TgsBearerHandler(
	IOptionsMonitor<TgsBearerOptions> optionsMonitor,
	ILoggerFactory loggerFactory,
	UrlEncoder urlEncoder)
	: AuthenticationHandler<TgsBearerOptions>(optionsMonitor, loggerFactory, urlEncoder)
{
	private static readonly AuthenticateResult _ValidatorNotFound = AuthenticateResult.Fail("No SecurityTokenValidator available for token.");

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		try
		{
			var token = GetBearerTokenOrNull();
			if (string.IsNullOrEmpty(token))
			{
				return AuthenticateResult.NoResult();
			}

			TokenValidationParameters tvp = await SetupTokenValidationParametersAsync().ConfigureAwait(false);
			List<Exception> validationFailures = null!;
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

					validationFailures ??= new List<Exception>(1);

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
				Logger.TokenValidationSucceeded();

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

				var ticket = new AuthenticationTicket(principal, properties, Scheme.Name);

				return AuthenticateResult.Success(ticket);
			}

			return validationFailures is not null
				? AuthenticateResult.Fail(validationFailures.Count == 1
					? validationFailures[0]
					: new AggregateException(validationFailures))
				: _ValidatorNotFound;
		}
		catch (Exception ex)
		{
			Logger.ErrorProcessingMessage(ex);

			throw;
		}
	}

	private static DateTime? GetSafeDateTime(DateTime dateTime)
		=> dateTime == DateTime.MinValue ? null : dateTime;

	private string? GetBearerTokenOrNull()
	{
		var authorization = Request.Headers.Authorization.ToString();

		return authorization.StartsWith("Bearer ", StringComparison.Ordinal)
			? authorization["Bearer ".Length..]
			: null;
	}

	private void RecordTokenValidationError(Exception exception, List<Exception> exceptions)
	{
		if (exception != null)
		{
			Logger.TokenValidationFailed(exception);
			exceptions.Add(exception);
		}

		if (Options.RefreshOnIssuerKeyNotFound && Options.ConfigurationManager != null && exception is SecurityTokenSignatureKeyNotFoundException)
			Options.ConfigurationManager.RequestRefresh();
	}

	private async Task<TokenValidationParameters> SetupTokenValidationParametersAsync()
	{
		var tokenValidationParameters = Options.TokenValidationParameters.Clone();
		if (Options.ConfigurationManager is BaseConfigurationManager configurationManager)
			tokenValidationParameters.ConfigurationManager = configurationManager;

		if (Options.ConfigurationManager is not null)
		{
			var tokenConfiguration = await Options.ConfigurationManager.GetConfigurationAsync(
				Context.RequestAborted).ConfigureAwait(false);

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