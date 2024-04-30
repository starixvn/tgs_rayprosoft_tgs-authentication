using Dapr.Actors;

namespace Tgs.Authentication.Actors;

public interface IAccountActor : IActor
{
    Task ChangePasswordAsync(string newPassword, string modifier, CancellationToken cancellationToken = default);

    Task DeleteAsync(string modifier, CancellationToken cancellationToken = default);

    Task<Account?> GetInfoAsync(CancellationToken cancellationToken = default);
}
