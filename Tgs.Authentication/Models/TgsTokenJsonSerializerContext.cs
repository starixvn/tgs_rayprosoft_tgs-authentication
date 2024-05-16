using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace Tgs.Authentication.Models;

[JsonSerializable(typeof(AccessTokenResponse))]
internal sealed partial class TgsTokenJsonSerializerContext : JsonSerializerContext
{
}