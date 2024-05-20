using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Tgs.Authentication.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IHttpClientBuilder AddTgsAuthenticationClient(this IServiceCollection services, Action<TgsBearerOptions, IServiceProvider> configure)
	{
		_ = services.AddOptions();
		services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TgsBearerOptions>, TgsBearerConfigureOptions>());

		return services
			.AddSingleton<TokenConfigurationRetriever>()
			.AddSingleton<TgsAuthenticateManager>()
			.AddOptions<TgsBearerOptions>()
			.Configure(configure)
			.Services
			.AddHttpClient<IAuthenticationClient, AuthenticationClient>();
	}

	public static IHttpClientBuilder AddTgsAuthenticationClient(this IServiceCollection services)
		=> services.AddTgsAuthenticationClient((_, _) => { });
}