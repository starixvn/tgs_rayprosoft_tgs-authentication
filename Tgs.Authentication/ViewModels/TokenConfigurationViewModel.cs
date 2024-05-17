using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace Tgs.Authentication.ViewModels;

public class TokenConfigurationViewModel
{
    public string? Issuer { get; set; }

    [JsonPropertyName("jwk")]
    public required JsonWebKey JsonWebKey { get; set; }
}