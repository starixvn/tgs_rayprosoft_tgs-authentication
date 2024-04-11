using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.ApplicationServices;

internal class DeleteAcccountCommandService(
	ITgsRepository tgsRepository,
	ISerialNumberGenerator serialNumberGenerator)
	: ICommandService<DeleteAcccountCommand>
{
	public async ValueTask ExecuteAsync(DeleteAcccountCommand command, CancellationToken cancellationToken = default)
	{
		var originSnapshot = await tgsRepository.GetAccountAsync(
			command.AccountId,
			cancellationToken).ConfigureAwait(false);

		if (originSnapshot is null)
			return;

		var eventOccurTime = DateTime.UtcNow;
		var changeEventId = await serialNumberGenerator.GenerateAsync(
			cancellationToken).ConfigureAwait(false);

		var changeEvent = originSnapshot.CreateDeleteEvent(changeEventId, eventOccurTime);

		await tgsRepository.AddAccountEventAsync(
			changeEvent,
			cancellationToken).ConfigureAwait(false);
		await tgsRepository.DeleteAccountAsync(
			command.AccountId,
			cancellationToken).ConfigureAwait(false);
	}
}
