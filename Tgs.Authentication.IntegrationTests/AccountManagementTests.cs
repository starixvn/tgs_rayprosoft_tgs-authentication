using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tgs.Authentication.Client;
using Tgs.Authentication.Repositories.Entities;
using Tgs.Authentication.Repositories.Entities.tgs2Schema;
using Xunit.Abstractions;

namespace Tgs.Authentication.IntegrationTests;

public class AccountManagementTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 整合測試_建立帳號()
    {
        // Arrange
        await using var web = AuthenticationTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper)));

        var client = web.CreateDefaultClient();

        var sut = new AuthenticationClient(client);

        var system = "test";
        var identityName = "test:Admin";
        var password = "1234";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        // Act
        var createdId = await sut.CreateAccountAsync(system, identityName, password);
        var actualRow = await dbContext.AccountRows.SingleAsync();

        // Assert
        Assert.Equal(createdId, actualRow.Id);
        Assert.Equal(1, await dbContext.AccountEventRows.CountAsync());
    }

    [Fact]
    public async Task 整合測試_刪除帳號()
    {
        // Arrange
        await using var web = AuthenticationTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper)));

        var client = web.CreateDefaultClient();

        var sut = new AuthenticationClient(client);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var accountId = Guid.NewGuid();

        _ = await dbContext.AccountEventRows.AddAsync(new AccountEventRow
        {
            Id = Guid.Empty,
            AccountId = accountId,
            Action = nameof(ChangeAction.Modify),
            ChangeSets = "{}",
            Operator = "system"
        });

        _ = await dbContext.AccountRows.AddAsync(new AccountRow
        {
            Id = accountId,
            IdentityName = "test:Admin",
            System = "test",
            Algorithm = "aaa",
        });

        _ = await dbContext.SaveChangesAsync();

        // Act
        await sut.DeleteAccountAsync(accountId);

        // Assert
        Assert.Equal(0, await dbContext.AccountRows.CountAsync());
        Assert.Equal(2, await dbContext.AccountEventRows.CountAsync());
    }

    [Fact]
    public async Task 整合測試_變更密碼()
    {
        // Arrange
        await using var web = AuthenticationTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper)));

        var client = web.CreateDefaultClient();

        var sut = new AuthenticationClient(client);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var accountId = Guid.NewGuid();

        _ = await dbContext.AccountEventRows.AddAsync(new AccountEventRow
        {
            Id = Guid.Empty,
            AccountId = accountId,
            Action = nameof(ChangeAction.Modify),
            ChangeSets = "{}",
            Operator = "system"
        });

        _ = await dbContext.AccountRows.AddAsync(new AccountRow
        {
            Id = accountId,
            IdentityName = "test:Admin",
            System = "test",
            Algorithm = "aaa",
        });

        _ = await dbContext.SaveChangesAsync();

        // Act
        await sut.ChangeAccountPassword(accountId, "1234");
        var actualRow = await dbContext.AccountRows.SingleAsync();

        // Assert
        Assert.NotNull(actualRow.PasswordHash);
        Assert.Equal(2, await dbContext.AccountEventRows.CountAsync());
    }

    [Fact]
    public async Task 整合測試_取得帳號資訊()
    {
        // Arrange
        await using var web = AuthenticationTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper)));

        var client = web.CreateDefaultClient();

        var sut = new AuthenticationClient(client);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var accountId = Guid.NewGuid();

        _ = await dbContext.AccountEventRows.AddAsync(new AccountEventRow
        {
            Id = Guid.Empty,
            AccountId = accountId,
            Action = nameof(ChangeAction.Modify),
            ChangeSets = "{}",
            Operator = "system"
        });

        _ = await dbContext.AccountRows.AddAsync(new AccountRow
        {
            Id = accountId,
            IdentityName = "test:Admin",
            System = "test",
            Algorithm = "aaa",
        });

        _ = await dbContext.SaveChangesAsync();

        // Act
        var actual = await sut.GetAccountAsync(accountId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(accountId, actual.Id);
    }
}
