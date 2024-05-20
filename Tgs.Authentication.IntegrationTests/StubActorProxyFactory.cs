using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Authentication.Actors;
using System.Reflection;

namespace Tgs.Authentication.IntegrationTests;

internal class StubActorProxyFactory(IServiceProvider serviceProvider)
    : IActorProxyFactory
{
    public TActorInterface CreateActorProxy<TActorInterface>(
        ActorId actorId,
        string actorType,
        ActorProxyOptions options)
        where TActorInterface : IActor
        => (TActorInterface)CreateActorProxy(
            actorId,
            typeof(TActorInterface),
            actorType,
            options);

    public object CreateActorProxy(ActorId actorId, Type actorInterfaceType, string actorType, ActorProxyOptions options)
    {
        if (actorInterfaceType.IsAssignableFrom(typeof(AccountActor)))
        {
            var actor = ActivatorUtilities.CreateInstance<AccountActorForTest>(
                serviceProvider.CreateScope().ServiceProvider,
                ActorHost.CreateForTest<AccountActor>(new ActorTestOptions
                {
                    ActorId = actorId
                }));
            actor.Activate();

            return actor;
        }
        else if (actorInterfaceType.IsAssignableFrom(typeof(RefreshTokenActor)))
        {
            var actor = ActivatorUtilities.CreateInstance<RefreshTokenActorForTest>(
                serviceProvider.CreateScope().ServiceProvider,
                ActorHost.CreateForTest<AccountActor>(new ActorTestOptions
                {
                    ActorId = actorId
                }));
            actor.Activate();

            return actor;
        }
        else
            throw new NotImplementedException();
    }

    public ActorProxy Create(ActorId actorId, string actorType, ActorProxyOptions options)
        => throw new NotImplementedException();
}
