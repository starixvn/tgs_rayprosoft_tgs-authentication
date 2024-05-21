using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Tgs.Authentication.Client;

internal class TgsBearerHandler(
	TgsAuthenticateManager tgsAuthenticateManager,
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

			var validationFailures = new List<Exception>(1);

			var ticket = await tgsAuthenticateManager.ValidateTokenAsync(
				token,
				Scheme.Name,
				validationFailures,
				Context.RequestAborted).ConfigureAwait(false);

			return ticket is not null
				? AuthenticateResult.Success(ticket)
				: validationFailures.Count > 0
					? AuthenticateResult.Fail(validationFailures.Count == 1
						? validationFailures[0]
						: new AggregateException(validationFailures))
					: _ValidatorNotFound;
		}
		catch (Exception ex)
		{
			Logger.ErrorProcessingMessage(ex);

			return AuthenticateResult.Fail(ex);
		}
	}

	private string? GetBearerTokenOrNull()
	{
		var authorization = Request.Headers.Authorization.ToString();

		return authorization.StartsWith("Bearer ", StringComparison.Ordinal)
			? authorization["Bearer ".Length..]
			: null;
	}
}