using Dapr.Actors.Runtime;
using Tgs.Authentication.Actors;
using Tgs.ServiceUtility;

namespace Tgs.Authentication.IntegrationTests;

internal class AccountActorForTest(ActorHost host, IServiceCoordinator serviceCoordinator)
    : AccountActor(host, serviceCoordinator)
{
    internal void Activate() => OnActivateAsync().Wait();
}
