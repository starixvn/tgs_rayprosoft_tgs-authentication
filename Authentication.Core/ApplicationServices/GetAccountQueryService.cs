using Tgs.ServiceUtility;

namespace Tgs.Authentication.ApplicationServices;

internal class GetAccountQueryService(
	ITgsRepository tgsRepository) : IGetService<GetAccountQuery, Account?>
{
	public async ValueTask<Account?> GetAsync(GetAccountQuery query, CancellationToken cancellationToken = default)
	{
		var originSnapshot = await tgsRepository.GetAccountAsync(
			query.AccountId,
			cancellationToken).ConfigureAwait(false);

		return originSnapshot is null
			? null
			: new Account(
			originSnapshot.Id,
			originSnapshot.UserName,
			originSnapshot.BrandId,
			originSnapshot.System,
			originSnapshot.Algorithm,
			originSnapshot.PasswordHash);
	}
}
