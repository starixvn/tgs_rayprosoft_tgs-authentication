namespace Tgs.Authentication.ViewModels;

public class CreateAccountViewModel
{
    public required string System { get; set; }

    public required string IdentityName { get; set; }

    public required string Password { get; set; }
}
