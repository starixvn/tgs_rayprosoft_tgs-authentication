using Dapr.Actors;

namespace Tgs.Authentication.Actors;

public interface IRefreshTokenActor : IActor
{
    Task<TgsTicketModel?> GetRefreshTicketAsync(CancellationToken cancellationToken = default);

    Task SetUsedAsync(CancellationToken cancellationToken = default);

    Task RollbackUsingAsync(CancellationToken cancellationToken = default);
}