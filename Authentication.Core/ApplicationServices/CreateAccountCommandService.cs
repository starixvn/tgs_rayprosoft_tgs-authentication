using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.ApplicationServices;

internal class CreateAccountCommandService(
	ITgsRepository tgsRepository,
	IHashAlgorithmFactory hashAlgorithmFactory,
	ISerialNumberGenerator serialNumberGenerator)
	: IGetService<CreateAccountCommand, Guid>
{
	public async ValueTask<Guid> GetAsync(CreateAccountCommand command, CancellationToken cancellationToken = default)
	{
		var hasher = hashAlgorithmFactory.GetDefaultHashAlgorithm();

		var accountSnapshot = new AccountSnapshot(
			Guid.NewGuid(),
			command.UserName,
			command.BrandId,
			command.System,
			hashAlgorithmFactory.DefaultAlgorithmName,
			await hasher.ComputeHashAsync(
				command.Password,
				cancellationToken).ConfigureAwait(false),
			DateTime.UtcNow);

		var accountEvent = accountSnapshot.CreateChangeEvent(
			await serialNumberGenerator.GenerateAsync(
				cancellationToken).ConfigureAwait(false),
			null,
			accountSnapshot.CreatedOn);

		await tgsRepository.AddAccountEventAsync(
			accountEvent,
			cancellationToken).ConfigureAwait(false);

		await tgsRepository.UpsertAccountAsync(
			accountSnapshot,
			accountEvent.Id,
			accountEvent.CreatedOn,
			cancellationToken).ConfigureAwait(false);

		return accountSnapshot.Id;
	}
}
