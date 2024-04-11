using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Authentication;

internal static class AuthenticationUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = false,
		Converters = {
			new JsonStringEnumConverter()
		},
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
	};
}
