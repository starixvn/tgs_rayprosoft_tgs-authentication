using Dapr.Actors.Runtime;
using Tgs.Authentication.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.Actors;

public class AccountActor(ActorHost host, IServiceCoordinator serviceCoordinator)
    : Actor(host)
    , IAccountActor
{
    private Guid _accountId;

    public async Task DeleteAsync(string modifier, CancellationToken cancellationToken = default)
    {
        var command = new DeleteAcccountCommand(_accountId, modifier);

        await serviceCoordinator.ExecuteAsync(
            command,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task ChangePasswordAsync(string newPassword, string modifier, CancellationToken cancellationToken = default)
    {
        var command = new ChangePasswordCommand(_accountId, newPassword, modifier);

        await serviceCoordinator.ExecuteAsync(
            command,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<AccountModel?> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetAccountQuery(_accountId);

        return await serviceCoordinator.GetAsync<GetAccountQuery, Account?>(
            query,
            cancellationToken)
            .AsTask()
            .ToAsyncEnumerable()
            .Where(account => account is not null)
            .Select(account => new AccountModel
            {
                Id = account!.Id,
                IdentityName = account.IdentityName
            })
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    protected override Task OnActivateAsync()
    {
        _accountId = Guid.Parse(Host.Id.GetId());

        Logger.LogInformation("Activate {AccountId}", _accountId);

        return Task.CompletedTask;
    }

    protected override Task OnDeactivateAsync()
    {
        Logger.LogInformation("Deactivate {AccountId}", _accountId);
        return Task.CompletedTask;
    }
}
