namespace Tgs.Authentication;

public interface ITgsRepository
{
	ValueTask<Account?> GetLoginAccountAsync(
		string system,
		Guid brandId,
		string userName,
		CancellationToken cancellationToken = default);

	ValueTask<AccountSnapshot?> GetAccountAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask AddAccountEventAsync(
		AccountEvent accountEvent,
		CancellationToken cancellationToken = default);

	ValueTask UpsertAccountAsync(
		AccountSnapshot snapshot,
		Guid snapshotId,
		DateTime updatedOn,
		CancellationToken cancellationToken = default);

	ValueTask DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default);
}
