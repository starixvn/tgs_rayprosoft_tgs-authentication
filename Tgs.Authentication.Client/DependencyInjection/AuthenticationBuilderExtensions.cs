using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Tgs.Authentication.Client.DependencyInjection;

public static class AuthenticationBuilderExtensions
{
	public static AuthenticationBuilder AddTgsBearer(this AuthenticationBuilder builder)
		=> builder
			.AddTgsBearer(TgsBearerDefaults.AuthenticationScheme, _ => { });

	public static AuthenticationBuilder AddTgsBearer(this AuthenticationBuilder builder, string authenticationScheme)
		=> builder
			.AddTgsBearer(authenticationScheme, _ => { });

	public static AuthenticationBuilder AddTgsBearer(this AuthenticationBuilder builder, Action<TgsBearerOptions> configuration)
		=> builder
			.AddTgsBearer(TgsBearerDefaults.AuthenticationScheme, configuration);

	public static AuthenticationBuilder AddTgsBearer(
		this AuthenticationBuilder builder,
		string authenticationScheme,
		Action<TgsBearerOptions> configuration)
	{
		builder.Services
			.AddKeyedSingleton<TgsAuthenticateManager>(authenticationScheme);

		return builder
			.AddScheme<TgsBearerOptions, TgsBearerHandler>(authenticationScheme, configuration);
	}
}