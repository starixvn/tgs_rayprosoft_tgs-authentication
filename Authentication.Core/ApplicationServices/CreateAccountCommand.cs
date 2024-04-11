namespace Tgs.Authentication.ApplicationServices;
public record CreateAccountCommand(
	string System,
	Guid BrandId,
	string UserName,
	string Password);
