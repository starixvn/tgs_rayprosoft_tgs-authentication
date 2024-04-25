using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.ApplicationServices;

internal class ChangePasswordCommandService(
	ITgsRepository tgsRepository,
	IHashAlgorithmFactory hashAlgorithmFactory,
	ISerialNumberGenerator serialNumberGenerator)
	: ICommandService<ChangePasswordCommand>
{
	public async ValueTask ExecuteAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
	{
		var originSnapshot = await tgsRepository.GetAccountAsync(
			command.AccountId,
			cancellationToken).ConfigureAwait(false);

		if (originSnapshot is null)
			return;

		var hasher = hashAlgorithmFactory.GetDefaultHashAlgorithm();

		var newSnapshot = originSnapshot with
		{
			Algorithm = hasher.AlgorithmName,
			PasswordHash = await hasher.ComputeHashAsync(
				command.NewPassword,
				cancellationToken).ConfigureAwait(false)
		};

		var eventOccurTime = DateTime.UtcNow;
		var changeEventId = await serialNumberGenerator.GenerateAsync(
			cancellationToken).ConfigureAwait(false);
		var changeEvent = newSnapshot.CreateChangeEvent(
			changeEventId,
			originSnapshot,
			eventOccurTime,
			command.Operator);

		await tgsRepository.AddAccountEventAsync(
			changeEvent,
			cancellationToken).ConfigureAwait(false);
		await tgsRepository.UpsertAccountAsync(
			newSnapshot,
			changeEventId,
			eventOccurTime,
			cancellationToken).ConfigureAwait(false);
	}
}
