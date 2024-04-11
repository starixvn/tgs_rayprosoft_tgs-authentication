namespace Tgs.Authentication.ViewModels;

public class CreateAccountViewModel
{
	public required string System { get; set; }

	public required Guid BrandId { get; set; }

	public required string UserName { get; set; }

	public required string Password { get; set; }
}
