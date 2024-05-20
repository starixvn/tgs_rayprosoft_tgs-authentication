namespace Tgs.Authentication;

public interface ITokenRepository
{
	ValueTask SaveTicketAsync(TgsTicket ticket, CancellationToken cancellationToken = default);

	ValueTask<TgsTicket?> GetTicketAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask EnableTicketAsync(Guid id, bool isEnabled, CancellationToken cancellationToken = default);
}