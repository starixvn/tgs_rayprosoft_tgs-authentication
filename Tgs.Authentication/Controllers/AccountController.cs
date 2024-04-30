using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;
using Tgs.Authentication.Actors;
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
    /// <param name="actorProxyFactory">The actor proxy factory.</param>
    /// <param name="accountId">要刪除的帳號ID</param>
    /// <returns></returns>
    [HttpDelete("{accountId:Guid}", Name = "DeleteAccount")]
    public async ValueTask DeleteAccountAsync(
        [FromServices] IActorProxyFactory actorProxyFactory,
        Guid accountId)
    {
        var actorId = new ActorId(accountId.ToString());
        var actor = actorProxyFactory.CreateActorProxy<IAccountActor>(actorId, ActorConsts.AccountActorTypeName);

        await actor.DeleteAsync(
            "system",
            HttpContext.RequestAborted).ConfigureAwait(false);
    }

    /// <summary>
    /// 變更密碼
    /// </summary>
    /// <param name="actorProxyFactory">The actor proxy factory.</param>
    /// <param name="accountId">帳號ID</param>
    /// <param name="password">要變更的密碼</param>
    /// <returns></returns>
    [HttpPatch("{accountId:Guid}", Name = "ChangePassword")]
    public async ValueTask ChangePasswordAsync(
        [FromServices] IActorProxyFactory actorProxyFactory,
        Guid accountId,
        [FromBody] string password)
    {
        var actorId = new ActorId(accountId.ToString());
        var actor = actorProxyFactory.CreateActorProxy<IAccountActor>(actorId, ActorConsts.AccountActorTypeName);

        await actor.ChangePasswordAsync(
            password,
            "system",
            HttpContext.RequestAborted).ConfigureAwait(false);
    }

    /// <summary>
    /// 取得帳號資訊
    /// </summary>
    /// <param name="actorProxyFactory">The actor proxy factory.</param>
    /// <param name="accountId">帳號ID</param>
    /// <returns></returns>
    [HttpGet("{accountId:Guid}", Name = "GetAccount")]
    public async Task<AccountViewModel?> GetAccountAsync(
        [FromServices] IActorProxyFactory actorProxyFactory,
        Guid accountId)
    {
        var actorId = new ActorId(accountId.ToString());
        var actor = actorProxyFactory.CreateActorProxy<IAccountActor>(actorId, ActorConsts.AccountActorTypeName);

        var result = await actor.GetInfoAsync(
            HttpContext.RequestAborted).ConfigureAwait(false);

        return result is null
            ? null
            : new AccountViewModel
            {
                Id = result.Id,
                IdentityName = result.IdentityName
            };
    }
}
