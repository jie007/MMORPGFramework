using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Protocols.Map;
using CommonServer.MapPartitioning;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace MapActorService.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IMapActorService : IActor
    {
        Task UpdatePlayerPosition(PositionMessage message);

        Task<List<PositionMessage>> GetPlayer(List<MapPartition> partitions);
        Task RemovePlayer(string name);
    }
}
