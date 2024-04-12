using Tgs.ServiceUtility;

namespace Tgs.Authentication.ApplicationServices;

internal class LoginCommandService(
	ITgsRepository tgsRepository,
	IHashAlgorithmFactory hashAlgorithmFactory)
	: IGetService<LoginCommand, Account?>
{
	public async ValueTask<Account?> GetAsync(LoginCommand command, CancellationToken cancellationToken = default)
	{
		var account = await tgsRepository.GetLoginAccountAsync(
			command.System,
			command.BrandId,
			command.IdentityName,
			cancellationToken).ConfigureAwait(false);

		if (account is null)
			return null;

		var hasher = hashAlgorithmFactory.GetHashAlgorithm(account.Algorithm);

		var validateResult = await hasher.ValidateAsync(
			command.Password,
			account.PasswordHash,
			cancellationToken).ConfigureAwait(false);

		return validateResult ? account : null;
	}
}
