using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Tgs.Authentication.ApplicationServices;
using Tgs.Authentication.Models;
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
            viewModel.IdentityName,
            viewModel.Password);

        var loginAccount = await loginService.GetAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (loginAccount is null)
            return Forbid();

        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, loginAccount.IdentityName),
                    new Claim(TgsClaimTypes.AccountId, loginAccount.Id.ToString()),
                    new Claim(TgsClaimTypes.SessionId, Guid.NewGuid().ToString()),
                    new Claim(TgsClaimTypes.System, system)
                ],
                BearerTokenDefaults.AuthenticationScheme));

        return SignIn(claimsPrincipal);
    }

    [HttpPost(":refresh", Name = "Refresh")]
    public async Task<IActionResult> RefreshAsync(
        [FromServices] IGetService<GetRefreshTicketQuery, TgsTicket?> getService,
        [FromServices] TicketDataProtector ticketDataProtector,
        [FromBody] RefreshRequest refreshRequest)
    {
        var refreshTokenId = Guid.Parse(refreshRequest.RefreshToken);

        var query = new GetRefreshTicketQuery(refreshTokenId);

        var tgsTicket = await getService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (tgsTicket is null)
            return Forbid();

        var ticket = TicketSerializer.Default.Deserialize(ticketDataProtector.Unprotect(tgsTicket.TicketData));

        return ticket is null
            ? Forbid()
            : SignIn(ticket.Principal, ticket.Properties, BearerTokenDefaults.AuthenticationScheme);
    }
}