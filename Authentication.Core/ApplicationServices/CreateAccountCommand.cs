namespace Tgs.Authentication.ApplicationServices;
public record CreateAccountCommand(
	string System,
	Guid BrandId,
	string IdentityName,
	string Password);
