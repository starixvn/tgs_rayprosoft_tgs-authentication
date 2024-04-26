using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Authentication.Client;

internal class AuthenticationClient(HttpClient httpClient)
	: IAuthenticationClient
{
	private static readonly JsonSerializerOptions _SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = false,
		Converters = {
			new JsonStringEnumConverter()
		},
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
	};

	public async ValueTask<Guid> CreateAccountAsync(string system, string identityName, string password, CancellationToken cancellationToken = default)
		=> await httpClient.PutAsJsonAsync(
			"/account",
			new
			{
				System = system,
				IdentityName = identityName,
				Password = password
			},
			_SerializerOptions,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<Guid>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable())
			.Take(1)
			.SingleAsync(cancellationToken)
			.ConfigureAwait(false);

	public async ValueTask DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
		=> await httpClient.DeleteAsync($"/account/{accountId}", cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.Take(1)
			.SingleAsync(cancellationToken)
			.ConfigureAwait(false);

	public async ValueTask ChangeAccountPassword(Guid accountId, string newPassword, CancellationToken cancellationToken = default)
		=> await httpClient.PatchAsJsonAsync($"/account/{accountId}", newPassword, cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.Take(1)
			.SingleAsync(cancellationToken)
			.ConfigureAwait(false);

	public async ValueTask<Account?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
		=> await httpClient.GetAsync($"/account/{accountId}", cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.Take(1)
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<Account>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable())
			.FirstOrDefaultAsync(cancellationToken)
			.ConfigureAwait(false);
}
