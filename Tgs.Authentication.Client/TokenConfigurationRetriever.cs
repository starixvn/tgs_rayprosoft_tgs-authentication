using Microsoft.IdentityModel.Protocols;

namespace Tgs.Authentication.Client;

internal class TokenConfigurationRetriever(IAuthenticationClient authenticationClient) : IConfigurationRetriever<TokenConfiguration>
{
	public Task<TokenConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
		=> authenticationClient.GetJsonWebKeyAsync(cancel).AsTask();
}