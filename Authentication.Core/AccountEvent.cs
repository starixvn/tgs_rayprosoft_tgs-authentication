using System.Text.Json;

namespace Tgs.Authentication;
public record AccountEvent(
	Guid Id,
	Guid AccountId,
	ChangeAction Action,
	JsonDocument ChangeSets,
	DateTime CreatedOn,
	string Operator);
