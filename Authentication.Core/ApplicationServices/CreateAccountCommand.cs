namespace Tgs.Authentication.ApplicationServices;
public record CreateAccountCommand(
	string System,
	string IdentityName,
	string Password,
	string Operator);
