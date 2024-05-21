using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Tgs.Authentication.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IHttpClientBuilder AddTgsAuthenticationClient(this IServiceCollection services)
	{
		_ = services.AddOptions();
		services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TgsBearerOptions>, TgsBearerConfigureOptions>());

		return services
			.AddSingleton<TokenConfigurationRetriever>()
			.AddSingleton<TgsAuthenticateManager>()
			.AddHttpClient<IAuthenticationClient, AuthenticationClient>();
	}
}