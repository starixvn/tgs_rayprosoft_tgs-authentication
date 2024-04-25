using Microsoft.AspNetCore.Mvc;
using Tgs.Authentication.ApplicationServices;
using Tgs.Authentication.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    [HttpPut(Name = "CreateAccount")]
    public ValueTask<Guid> CreateAccountAsync(
        [FromServices] IGetService<CreateAccountCommand, Guid> createService,
        CreateAccountViewModel viewModel)
    {
        var command = new CreateAccountCommand(
            viewModel.System,
            viewModel.BrandId,
            viewModel.UserName,
            viewModel.Password,
            "system");

        return createService.GetAsync(command, HttpContext.RequestAborted);
    }

    [HttpDelete("{accountId:Guid}", Name = "DeleteAccount")]
    public ValueTask DeleteAccountAsync(
        [FromServices] ICommandService<DeleteAcccountCommand> deleteService,
        Guid accountId)
    {
        var command = new DeleteAcccountCommand(accountId, "system");

        return deleteService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpPatch("{accountId:Guid}", Name = "ChangePassword")]
    public ValueTask ChangePasswordAsync(
        [FromServices] ICommandService<ChangePasswordCommand> changePasswordService,
        Guid accountId,
        [FromBody] string password)
    {
        var command = new ChangePasswordCommand(accountId, password, "system");

        return changePasswordService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpGet("{accountId:Guid}", Name = "GetAccount")]
    public async Task<AccountViewModel?> GetAccountAsync(
        [FromServices] IGetService<GetAccountQuery, Account?> getService,
        Guid accountId)
    {
        var query = new GetAccountQuery(accountId);

        var result = await getService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return result is null
            ? null
            : new AccountViewModel
            {
                Id = result.Id,
                UserName = result.IdentityName
            };
    }
}
