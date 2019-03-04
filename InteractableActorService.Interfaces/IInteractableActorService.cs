using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Protocols.Interactable;
using Common.Protocols.Map;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace InteractableActorService.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IInteractableActorService : IActor
    {
        Task StartInteraction(string name, StartInteractMessage msg);

        Task<bool> FinishInteraction(string name, FinishInteractMessage msg);

        Task<List<InteractableStatusMessage>> GetAllStatus();

        Task<List<InteractableStatusMessage>> GetUpdateStatus(DateTime fromTime);
    }
}
