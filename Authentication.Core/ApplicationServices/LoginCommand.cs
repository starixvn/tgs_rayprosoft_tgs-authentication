namespace Tgs.Authentication.ApplicationServices;
public record LoginCommand(
	string System,
	Guid BrandId,
	string UserName,
	string Password);
