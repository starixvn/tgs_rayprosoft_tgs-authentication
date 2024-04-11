using System.Text.Json;
using Json.Patch;
using Json.Pointer;

namespace Tgs.Authentication;
public record AccountSnapshot(
	Guid Id,
	string UserName,
	Guid BrandId,
	string System,
	string Algorithm,
	string PasswordHash,
	DateTime CreatedOn)
{
	public AccountEvent CreateChangeEvent(Guid eventId, AccountSnapshot? baseSnapshot, DateTime createdOn)
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
			createdOn);
	}

	public AccountEvent CreateDeleteEvent(Guid eventId, DateTime createdOn)
		=> new(
			eventId,
			Id,
			ChangeAction.Delete,
			JsonSerializer.SerializeToDocument(new JsonPatch()),
			createdOn);
}
