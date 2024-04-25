using System.Text.Json;
using Json.Patch;
using Json.Pointer;

namespace Tgs.Authentication;
public record AccountSnapshot(
	Guid Id,
	string IdentityName,
	string System,
	string Algorithm,
	string? PasswordHash,
	DateTime CreatedOn)
{
	public AccountEvent CreateChangeEvent(Guid eventId, AccountSnapshot? baseSnapshot, DateTime createdOn, string modifier)
	{
		var changeSets = baseSnapshot is null
			? new JsonPatch(PatchOperation.Add(
				JsonPointer.Create<AccountSnapshot>(o => o),
				JsonSerializer.SerializeToNode(this, AuthenticationUtility.SerializerOptions)))
			: baseSnapshot.CreatePatch(this, AuthenticationUtility.SerializerOptions);

		return new AccountEvent(
			eventId,
			Id,
			ChangeAction.Modify,
			JsonSerializer.SerializeToDocument(changeSets),
			createdOn,
			modifier);
	}

	public AccountEvent CreateDeleteEvent(Guid eventId, DateTime createdOn, string modifier)
		=> new(
			eventId,
			Id,
			ChangeAction.Delete,
			JsonSerializer.SerializeToDocument(new JsonPatch()),
			createdOn,
			modifier);
}
