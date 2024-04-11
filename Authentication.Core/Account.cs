namespace Tgs.Authentication;
public record Account(
	Guid Id,
	string UserName,
	Guid BrandId,
	string System,
	string Algorithm,
	string PasswordHash);
