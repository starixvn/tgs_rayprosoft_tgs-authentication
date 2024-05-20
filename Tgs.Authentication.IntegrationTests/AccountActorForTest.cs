using Dapr.Actors.Runtime;
using Tgs.Authentication.Actors;

namespace Tgs.Authentication.IntegrationTests;

internal class AccountActorForTest(ActorHost host, IServiceProvider serviceProvider)
    : AccountActor(host, serviceProvider)
{
    internal void Activate() => OnActivateAsync().Wait();
}
