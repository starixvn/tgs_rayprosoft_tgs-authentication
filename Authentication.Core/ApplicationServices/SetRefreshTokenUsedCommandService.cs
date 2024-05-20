using Tgs.ServiceUtility;

namespace Tgs.Authentication.ApplicationServices;

internal class SetRefreshTokenUsedCommandService(ITokenRepository tokenRepository) : ICommandService<SetRefreshTokenUsedCommand>
{
	public async ValueTask ExecuteAsync(SetRefreshTokenUsedCommand command, CancellationToken cancellationToken = default)
	{
		var ticket = await tokenRepository.GetTicketAsync(
			command.RefreshTokenId,
			cancellationToken).ConfigureAwait(false);

		if (ticket is null || ticket.Type is not TokenType.RefreshToken)
			return;

		await tokenRepository.EnableTicketAsync(
			command.RefreshTokenId,
			false,
			cancellationToken).ConfigureAwait(false);
	}
}