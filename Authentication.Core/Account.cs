namespace Tgs.Authentication;
public record Account(
	Guid Id,
	string IdentityName,
	string System,
	string Algorithm,
	string? PasswordHash);
