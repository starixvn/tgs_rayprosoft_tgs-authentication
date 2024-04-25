namespace Tgs.Authentication.ApplicationServices;
public record ChangePasswordCommand(
	Guid AccountId,
	string NewPassword,
	string Operator);
