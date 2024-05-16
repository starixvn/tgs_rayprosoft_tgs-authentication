using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client;

public interface IAuthenticationClient
{
	ValueTask<Guid> CreateAccountAsync(string system, string identityName, string password, CancellationToken cancellationToken = default);

	ValueTask DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

	ValueTask ChangeAccountPassword(Guid accountId, string newPassword, CancellationToken cancellationToken = default);

	ValueTask<AccessTokenResponse?> LoginAsync(string system, string ideneityName, string password, CancellationToken cancellationToken = default);

	ValueTask<JsonWebKey> GetJsonWebKeyAsync(CancellationToken cancellationToken = default);

	ValueTask<AccessTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}