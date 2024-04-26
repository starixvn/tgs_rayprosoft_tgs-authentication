using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.Authentication.Client;
public interface IAuthenticationClient
{
	ValueTask<Guid> CreateAccountAsync(string system, string identityName, string password, CancellationToken cancellationToken = default);
	ValueTask DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
	ValueTask ChangeAccountPassword(Guid accountId, string newPassword, CancellationToken cancellationToken = default);
}
