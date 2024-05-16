using Tgs.ServiceUtility;

namespace Tgs.Authentication.ApplicationServices;

internal class GetRefreshTicketQueryService(ITokenRepository tokenRepository)
	: IGetService<GetRefreshTicketQuery, TgsTicket?>
{
	public async ValueTask<TgsTicket?> GetAsync(GetRefreshTicketQuery query, CancellationToken cancellationToken = default)
	{
		var ticket = await tokenRepository.GetTicketAsync(
			query.RefreshTokenId,
			cancellationToken).ConfigureAwait(false);

		return ticket is null || ticket.Type is not TokenType.RefreshToken || !ticket.Enabled ? null : ticket;
	}
}