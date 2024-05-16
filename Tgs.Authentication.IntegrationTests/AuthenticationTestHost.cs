using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Tgs.Authentication.Repositories.Entities;

namespace Tgs.Authentication.IntegrationTests;

internal static class AuthenticationTestHost
{
    public static WebApplicationFactory<Program> Instance => new WebApplicationFactory<Program>()
        .WithWebHostBuilder(hostBuilder => hostBuilder
            .ConfigureAppConfiguration(config =>
            {
                var aesAlg = Aes.Create();

                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Identity:Key"] = Convert.ToBase64String(aesAlg.Key),
                    ["Identity:IV"] = Convert.ToBase64String(aesAlg.IV)
                });
            })
            .ConfigureLogging((_, logging) => logging.ClearProviders())
            .ConfigureServices(services => services
                .AddKeyedSingleton(
                    "IdentityKey",
                    (sp, _) =>
                    {
                        var rsa = RSA.Create();

                        return new RsaSecurityKey(rsa);
                    })
                .AddSingleton(sp => new TgsContext(
                    new DbContextOptionsBuilder<TgsContext>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .Options))));
}