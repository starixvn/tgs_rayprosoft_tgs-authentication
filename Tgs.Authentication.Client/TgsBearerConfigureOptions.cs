using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

namespace Tgs.Authentication.Client;

internal class TgsBearerConfigureOptions(TokenConfigurationRetriever configurationRetriever) : IConfigureNamedOptions<TgsBearerOptions>
{
	public void Configure(string? schemeName, TgsBearerOptions options)
	{
		if (string.IsNullOrEmpty(schemeName))
			return;

		options.ConfigurationManager = new ConfigurationManager<TokenConfiguration>("/session/jwk", configurationRetriever);
	}

	public void Configure(TgsBearerOptions options)
		=> throw new NotImplementedException();
}