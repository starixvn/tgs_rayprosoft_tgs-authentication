using System.Text.Json;
using Tgs.Authentication.Repositories.Entities;
using Tgs.Authentication.Repositories.Entities.tgs2Schema;

namespace Tgs.Authentication.Repositories;

internal class TgsRepository(TgsContext dbContext)
	: ITgsRepository
	, ITokenRepository
{
	public async ValueTask<Account?> GetLoginAccountAsync(string system, string identityName, CancellationToken cancellationToken = default)
	{
		return await dbContext.AccountRows
			.Where(row => row.IdentityName == identityName && row.System == system)
			.Take(1)
			.ToAsyncEnumerable()
			.Select(row => new Account(
				row.Id,
				row.IdentityName,
				row.System,
				row.Algorithm,
				row.PasswordHash))
			.FirstOrDefaultAsync(cancellationToken)
			.ConfigureAwait(false);
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

	public async ValueTask SaveTicketAsync(TgsTicket ticket, CancellationToken cancellationToken = default)
	{
		var newRow = new Token
		{
			Id = ticket.Id,
			Type = ticket.Type.ToString(),
			Ticket = ticket.TicketData,
			Expiry = ticket.Expiry.ToUniversalTime(),
			Enabled = ticket.Enabled,
			CreatedOn = DateTime.UtcNow
		};

		_ = await dbContext.Tokens.AddAsync(
			newRow,
			cancellationToken).ConfigureAwait(false);

		_ = await dbContext.SaveChangesAsync(
			cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<TgsTicket?> GetTicketAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var tokenRow = await dbContext.Tokens.FindAsync(
			[id],
			cancellationToken).ConfigureAwait(false);

		return tokenRow is null
			? null
			: new TgsTicket(
				tokenRow.Id,
				Enum.Parse<TokenType>(tokenRow.Type),
				tokenRow.Ticket,
				tokenRow.Expiry,
				tokenRow.Enabled);
	}
}