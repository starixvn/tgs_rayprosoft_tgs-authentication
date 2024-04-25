using Microsoft.AspNetCore.Mvc;
using Tgs.Authentication.ApplicationServices;
using Tgs.Authentication.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    /// <summary>
    /// 建立帳號
    /// </summary>
    /// <param name="createService">The create service.</param>
    /// <param name="viewModel">建立帳號資訊</param>
    /// <returns></returns>
    [HttpPut(Name = "CreateAccount")]
    public ValueTask<Guid> CreateAccountAsync(
        [FromServices] IGetService<CreateAccountCommand, Guid> createService,
        CreateAccountViewModel viewModel)
    {
        var command = new CreateAccountCommand(
            viewModel.System,
            viewModel.IdentityName,
            viewModel.Password,
            "system");

        return createService.GetAsync(command, HttpContext.RequestAborted);
    }

    /// <summary>
    /// 刪除帳號
    /// </summary>
    /// <param name="deleteService">The delete service.</param>
    /// <param name="accountId">要刪除的帳號ID</param>
    /// <returns></returns>
    [HttpDelete("{accountId:Guid}", Name = "DeleteAccount")]
    public ValueTask DeleteAccountAsync(
        [FromServices] ICommandService<DeleteAcccountCommand> deleteService,
        Guid accountId)
    {
        var command = new DeleteAcccountCommand(accountId, "system");

        return deleteService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    /// <summary>
    /// 變更密碼
    /// </summary>
    /// <param name="changePasswordService">The change password service.</param>
    /// <param name="accountId">帳號ID</param>
    /// <param name="password">要變更的密碼</param>
    /// <returns></returns>
    [HttpPatch("{accountId:Guid}", Name = "ChangePassword")]
    public ValueTask ChangePasswordAsync(
        [FromServices] ICommandService<ChangePasswordCommand> changePasswordService,
        Guid accountId,
        [FromBody] string password)
    {
        var command = new ChangePasswordCommand(accountId, password, "system");

        return changePasswordService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    /// <summary>
    /// 取得帳號資訊
    /// </summary>
    /// <param name="getService">The get service.</param>
    /// <param name="accountId">帳號ID</param>
    /// <returns></returns>
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
