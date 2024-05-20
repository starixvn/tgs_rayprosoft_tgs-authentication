using Dapr.Actors.Runtime;
using Tgs.Authentication.Actors;

namespace Tgs.Authentication.IntegrationTests;
internal class RefreshTokenActorForTest(ActorHost host, TimeProvider timeProvider, IServiceProvider serviceProvider)
    : RefreshTokenActor(host, timeProvider, serviceProvider)
{
    internal void Activate() => OnActivateAsync().Wait();
}
