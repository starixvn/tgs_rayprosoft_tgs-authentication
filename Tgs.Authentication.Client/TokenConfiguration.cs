using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.Client;

public record TokenConfiguration(
	string? Issuer,
	[property: JsonPropertyName("jwk")] JsonWebKey JsonWebKey);