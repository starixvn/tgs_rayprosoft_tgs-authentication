using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tgs.Authentication.Repositories.Entities;

namespace Tgs.Authentication.IntegrationTests;

internal static class AuthenticationTestHost
{
    public static WebApplicationFactory<Program> Instance => new WebApplicationFactory<Program>()
        .WithWebHostBuilder(hostBuilder => hostBuilder
            .ConfigureAppConfiguration(config => config.Sources.Clear())
            .ConfigureLogging((_, logging) => logging.ClearProviders())
            .ConfigureServices(services => services
                .AddSingleton(sp => new TgsContext(
                    new DbContextOptionsBuilder<TgsContext>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .Options))));
}
