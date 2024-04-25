using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Authentication.Repositories.Entities;

namespace Tgs.Authentication.Repositories.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTgsAuthenticationRepositories(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> dbConfigure)
		=> services
			.AddDbContext<TgsContext>(dbConfigure)
			.AddScoped<ITgsRepository, TgsRepository>();
}
