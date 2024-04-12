using Microsoft.AspNetCore.Authentication.BearerToken;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Authentication.ApplicationServices;
using Tgs.Authentication.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.Controllers;

[Route("[controller]")]
[ApiController]
public class SessionController : ControllerBase
{
    [HttpPost("{system}:login", Name = "Login")]
    public async Task<IActionResult> LoginAsync(
        [FromServices] IGetService<LoginCommand, Account?> loginService,
        string system,
        [FromBody] LoginViewModel viewModel)
    {
        var command = new LoginCommand(
            system,
            viewModel.BrandId,
            viewModel.UserName,
            viewModel.Password);

        var loginAccount = await loginService.GetAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (loginAccount is null)
            return Forbid();

        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, loginAccount.IdentityName),
                    new Claim("system", loginAccount.System),
                    new Claim("account_id", loginAccount.Id.ToString())
                ],
                BearerTokenDefaults.AuthenticationScheme));

        return SignIn(claimsPrincipal);
    }
}
