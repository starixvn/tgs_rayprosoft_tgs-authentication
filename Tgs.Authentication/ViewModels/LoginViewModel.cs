namespace Tgs.Authentication.ViewModels;

public class LoginViewModel
{
    public required Guid BrandId { get; set; }

    public required string UserName { get; set; }

    public required string Password { get; set; }
}
