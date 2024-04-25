namespace Tgs.Authentication.ApplicationServices;
public record LoginCommand(
	string System,
	string IdentityName,
	string? Password);
