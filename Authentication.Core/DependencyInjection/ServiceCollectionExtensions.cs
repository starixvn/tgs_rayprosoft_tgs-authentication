using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tgs.Authentication.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTgsAuthentication(this IServiceCollection services)
		=> services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddSingleton<IHashAlgorithmFactory, HashAlgorithmFactory>()
			.AddSerialNumberGenerator();
}
