using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Protocols.Map;
using CommonServer.MapPartitioning;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using MapActorService.Interfaces;

namespace MapActorService
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.None)]
    public class MapActorService : Actor, IMapActorService
    {
        private Dictionary<MapPartition, List<string>> PartitionPlayerMapping = new Dictionary<MapPartition, List<string>>();
        private Dictionary<string, PositionMessage> PlayerPositionMapping = new Dictionary<string, PositionMessage>();
        private Dictionary<string, DateTime> PlayerTimeouts = new Dictionary<string, DateTime>();

        /// <summary>
        /// Initializes a new instance of MapActorService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public MapActorService(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            return base.OnActivateAsync();
        }

        protected override Task OnDeactivateAsync()
        {
            PartitionPlayerMapping.Clear();
            return base.OnDeactivateAsync();
        }

        public async Task UpdatePlayerPosition(PositionMessage message)
        {
            UpdateTimeout(message);

            RemovePlayer(message.Name);

            PlayerPositionMapping.Add(message.Name, message);
            var newPartition = new MapPartition(message);
            if (PartitionPlayerMapping.ContainsKey(newPartition))
            {
                PartitionPlayerMapping[newPartition].Add(message.Name);
            }
            else
            {
                PartitionPlayerMapping.Add(newPartition, new List<string>()
                {
                    message.Name
                });
            }

        }

        private void UpdateTimeout(PositionMessage message)
        {
            if (PlayerTimeouts.ContainsKey(message.Name))
            {
                PlayerTimeouts[message.Name] = DateTime.Now;
            }
            else
            {
                PlayerTimeouts.Add(message.Name, DateTime.Now);
            }
        }

        public async Task RemovePlayer(string name)
        {
            if (PlayerPositionMapping.ContainsKey(name))
            {
                var oldPartition = new MapPartition(PlayerPositionMapping[name]);
                if (PartitionPlayerMapping.ContainsKey(oldPartition))
                {
                    PartitionPlayerMapping[oldPartition].Remove(name);
                }

                PlayerPositionMapping.Remove(name);
            }
        }

        private void RemoveTimedOutPlayers()
        {
            List<string> timeoutRemovals = new List<string>();
            foreach (var timeout in PlayerTimeouts)
            {
                if (DateTime.Now - timeout.Value >= TimeSpan.FromSeconds(GameDesign.TimeoutOnMap))
                {
                    RemovePlayer(timeout.Key);
                    timeoutRemovals.Add(timeout.Key);
                }
            }

            foreach (var remove in timeoutRemovals)
            {
                PlayerTimeouts.Remove(remove);
            }
        }

        public async Task<List<PositionMessage>> GetPlayer(List<MapPartition> partitions)
        {
            RemoveTimedOutPlayers();

            List<PositionMessage> positionMessages = new List<PositionMessage>();

            foreach (var partition in partitions)
            {
                if (!PartitionPlayerMapping.ContainsKey(partition))
                    continue;

                foreach (string player in PartitionPlayerMapping[partition])
                {
                    positionMessages.Add(PlayerPositionMapping[player]);
                }
            }

            return positionMessages;
        }
    }
}
