using System.Security.Claims;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Tgs.Authentication.Client;
using Tgs.Authentication.Models;
using Tgs.Authentication.Repositories.Entities;
using Tgs.Authentication.Repositories.Entities.tgs2Schema;
using Xunit;
using Xunit.Abstractions;

namespace Tgs.Authentication.IntegrationTests;

public class LoginSessionTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 整合測試_登入取得Token()
    {
        // Arrange
        await using var web = AuthenticationTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper)));

        var client = web.CreateDefaultClient();

        var sut = new AuthenticationClient(client);
        var system = "test";
        var identityName = "test:test1";
        var password = "1234";

        var hashFactory = web.Services.GetRequiredService<IHashAlgorithmFactory>();
        var hasher = hashFactory.GetDefaultHashAlgorithm();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.AccountRows.AddAsync(new AccountRow
        {
            Id = Guid.NewGuid(),
            System = system,
            IdentityName = identityName,
            Algorithm = hasher.AlgorithmName,
            PasswordHash = await hasher.ComputeHashAsync(password),
            CreatedOn = DateTime.UtcNow,
            SnapshotId = Guid.NewGuid(),
            UpdatedOn = DateTime.UtcNow
        });

        _ = await dbContext.SaveChangesAsync();

        // Act
        var actual = await sut.LoginAsync(system, identityName, password);

        // Assert
        Assert.NotNull(actual);
    }

    [Fact]
    public async Task 整合測試_取得jwk()
    {
        // Arrange
        await using var web = AuthenticationTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper)));

        var client = web.CreateDefaultClient();

        var sut = new AuthenticationClient(client);

        // Act
        var actual = await sut.GetJsonWebKeyAsync();

        // Assert
        Assert.NotNull(actual);
    }

    [Fact]
    public async Task 整合測試_更新AccessToken()
    {
        // Arrange
        await using var web = AuthenticationTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .Replace(ServiceDescriptor.Singleton<IActorProxyFactory, StubActorProxyFactory>())));

        var client = web.CreateDefaultClient();

        var sut = new AuthenticationClient(client);
        var refreshTokenId = Guid.NewGuid();

        var hashFactory = web.Services.GetRequiredService<IHashAlgorithmFactory>();
        var hasher = hashFactory.GetDefaultHashAlgorithm();

        var tickProtector = web.Services.GetRequiredService<TicketDataProtector>();
        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "test:test1"),
                    new Claim(TgsClaimTypes.AccountId, Guid.NewGuid().ToString()),
                    new Claim(TgsClaimTypes.SessionId, Guid.NewGuid().ToString()),
                    new Claim(TgsClaimTypes.System, "test")
                ],
                BearerTokenDefaults.AuthenticationScheme));
        var ticket = new AuthenticationTicket(claimsPrincipal, $"{BearerTokenDefaults.AuthenticationScheme}:RefreshToken");
        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Tokens.AddAsync(new Token
        {
            Id = refreshTokenId,
            Type = nameof(TokenType.RefreshToken),
            Ticket = tickProtector.Protect(TicketSerializer.Default.Serialize(ticket)),
            CreatedOn = DateTime.UtcNow,
            Enabled = true,
            Expiry = DateTime.UtcNow.AddDays(1D)
        });

        _ = await dbContext.SaveChangesAsync();

        // Act
        var actual = await sut.RefreshTokenAsync(refreshTokenId.ToString());

        // Assert
        Assert.NotNull(actual);
    }
}