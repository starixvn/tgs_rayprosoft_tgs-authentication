using System.Text.Json;
using Tgs.Authentication.Repositories.Entities;
using Tgs.Authentication.Repositories.Entities.tgs2Schema;

namespace Tgs.Authentication.Repositories;

internal class TgsRepository(TgsContext dbContext)
	: ITgsRepository
{
	public async ValueTask<Account?> GetLoginAccountAsync(string system, string identityName, CancellationToken cancellationToken = default)
	{
		var findRow = await dbContext.AccountRows.FindAsync(
			[identityName, system],
			cancellationToken).ConfigureAwait(false);

		return findRow is null
			? null
			: new Account(
				findRow.Id,
				findRow.IdentityName,
				findRow.System,
				findRow.Algorithm,
				findRow.PasswordHash);
	}

	public async ValueTask<AccountSnapshot?> GetAccountAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var findRow = await dbContext.AccountRows.FindAsync(
			[id],
			cancellationToken).ConfigureAwait(false);

		return findRow is null
			? null
			: new AccountSnapshot(
				findRow.Id,
				findRow.IdentityName,
				findRow.System,
				findRow.Algorithm,
				findRow.PasswordHash,
				findRow.CreatedOn);
	}

	public async ValueTask AddAccountEventAsync(AccountEvent accountEvent, CancellationToken cancellationToken = default)
	{
		var newEventRow = new AccountEventRow
		{
			Id = accountEvent.Id,
			AccountId = accountEvent.AccountId,
			Action = accountEvent.Action.ToString(),
			ChangeSets = JsonSerializer.Serialize(accountEvent.ChangeSets),
			CreatedOn = accountEvent.CreatedOn,
			Operator = accountEvent.Operator
		};

		_ = await dbContext.AccountEventRows.AddAsync(
			newEventRow,
			cancellationToken).ConfigureAwait(false);
		_ = await dbContext.SaveChangesAsync(
			  cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpsertAccountAsync(AccountSnapshot snapshot, Guid snapshotId, DateTime updatedOn, CancellationToken cancellationToken = default)
	{
		var findRow = await dbContext.AccountRows.FindAsync(
			[snapshot.Id],
			cancellationToken).ConfigureAwait(false)
			?? await dbContext.AccountRows.AddAsync(
				new AccountRow
				{
					Id = snapshot.Id,
					IdentityName = snapshot.IdentityName,
					System = snapshot.System,
					CreatedOn = snapshot.CreatedOn
				},
				cancellationToken)
				.AsTask()
				.ToAsyncEnumerable()
				.Take(1)
				.Select(e => e.Entity)
				.SingleAsync(cancellationToken)
				.ConfigureAwait(false);

		findRow.Algorithm = snapshot.Algorithm;
		findRow.PasswordHash = snapshot.PasswordHash;
		findRow.SnapshotId = snapshotId;
		findRow.UpdatedOn = updatedOn.ToUniversalTime();

		_ = await dbContext.SaveChangesAsync(
			cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var findRow = await dbContext.AccountRows.FindAsync(
			[id],
			cancellationToken).ConfigureAwait(false);

		if (findRow is not null)
		{
			dbContext.AccountRows.Remove(findRow);

			_ = await dbContext.SaveChangesAsync(
				cancellationToken).ConfigureAwait(false);
		}
	}
}
