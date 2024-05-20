using Dapr.Actors.Runtime;
using Tgs.Authentication.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.Actors;

public class RefreshTokenActor(ActorHost host, TimeProvider timeProvider, IServiceProvider serviceProvider)
    : Actor(host)
    , IRefreshTokenActor
{
    private Guid _refreshTokenId;
    private TgsTicket? _ticket;
    private DateTime? _getTicketTime;

    public Task<TgsTicketModel?> GetRefreshTicketAsync(CancellationToken cancellationToken = default)
    {
        var currentTime = timeProvider.GetUtcNow().LocalDateTime;

        if (_getTicketTime is not null && _getTicketTime.Value <= currentTime.AddMinutes(5D))
            return Task.FromResult<TgsTicketModel?>(null);

        _getTicketTime = currentTime;

        return Task.FromResult(_ticket is null
                ? null
                : new TgsTicketModel
                {
                    Id = _ticket.Id,
                    TicketData = _ticket.TicketData
                });
    }

    public async Task SetUsedAsync(CancellationToken cancellationToken = default)
    {
        var scope = serviceProvider.CreateAsyncScope();
        await using var _ = scope.ConfigureAwait(false);
        var serviceCoordinator = scope.ServiceProvider.GetRequiredService<IServiceCoordinator>();

        var command = new SetRefreshTokenUsedCommand(_refreshTokenId);

        await serviceCoordinator.ExecuteAsync(
            command,
            cancellationToken).ConfigureAwait(false);

        _ticket = null;
        _getTicketTime = null;
    }

    public Task RollbackUsingAsync(CancellationToken cancellationToken = default)
    {
        _getTicketTime = null;

        return Task.CompletedTask;
    }

    protected override async Task OnActivateAsync()
    {
        _refreshTokenId = Guid.Parse(Host.Id.GetId());

        var scope = serviceProvider.CreateAsyncScope();
        await using var _ = scope.ConfigureAwait(false);
        var serviceCoordinator = scope.ServiceProvider.GetRequiredService<IServiceCoordinator>();

        var query = new GetRefreshTicketQuery(_refreshTokenId);
        _ticket = await serviceCoordinator.GetAsync<GetRefreshTicketQuery, TgsTicket?>(
            query).ConfigureAwait(false);
    }
}