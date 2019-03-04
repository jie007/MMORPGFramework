using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ChatActorService.Interfaces;
using Common;
using Common.Interactables;
using Common.Protocols.Chat;
using Common.Protocols.Interactable;
using Common.Protocols.Map;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using InteractableActorService.Interfaces;
using Newtonsoft.Json;

namespace InteractableActorService
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
    internal class InteractableActorService : Actor, IInteractableActorService
    {
        private string dataPackagePath = string.Empty;
        private MapInformation mapInfo;

        private List<InteractableStatusMessage> status = new List<InteractableStatusMessage>();
        private Dictionary<DateTime, InteractableStatusMessage> statusChanges = new Dictionary<DateTime, InteractableStatusMessage>();
        private Dictionary<string, DateTime> runningInteractions = new Dictionary<string, DateTime>();
        private Dictionary<DateTime, string> respawnTime = new Dictionary<DateTime, string>();
        private IActorTimer respawnTimer;

        /// <summary>
        /// Initializes a new instance of InteractableActorService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public InteractableActorService(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            var dp = this.ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Data");
            dataPackagePath = dp.Path;
        }

        protected override Task OnActivateAsync()
        {
            string mapName = this.Id.GetStringId();

            mapInfo = JsonConvert.DeserializeObject<MapInformation>(
                File.ReadAllText(Path.Combine(dataPackagePath, mapName + ".json")));

            foreach (var interactable in mapInfo.Interactables)
            {
                status.Add(new InteractableStatusMessage(interactable.Id, true));
            }

            respawnTimer = RegisterTimer(RespawnInteractables, null,
                TimeSpan.FromMilliseconds(GameDesign.InteractableRespawnTimerDelay),
                TimeSpan.FromMilliseconds(GameDesign.InteractableRespawnTimerDelay));

            return base.OnActivateAsync();
        }

        protected override Task OnDeactivateAsync()
        {
            if (respawnTimer != null)
            {
                UnregisterTimer(respawnTimer);
            }

            return base.OnDeactivateAsync();
        }

        private async Task RespawnInteractables(object state)
        {
            var needRespawns = respawnTime.Where(x => x.Key <= DateTime.UtcNow).ToList();

            foreach (var needRespawn in needRespawns)
            {
                var currStatus = status.First(x => x.Id == needRespawn.Value);
                currStatus.IsActive = true;
                RegisterStatusChange(currStatus);

                respawnTime.Remove(needRespawn.Key);
            }
        }

        public async Task StartInteraction(string name, StartInteractMessage msg)
        {
            DateTime t = new DateTime(msg.ServerTimeStamp);
            if (!runningInteractions.ContainsKey(name))
                runningInteractions.Add(name, t);
            else
                runningInteractions[name] = t;
        }

        public async Task<bool> FinishInteraction(string name, FinishInteractMessage msg)
        {
            if (!runningInteractions.ContainsKey(name))
                return false;

            DateTime t = new DateTime(msg.ServerTimeStamp);
            var interactable = mapInfo.Interactables.First(x => x.Id == msg.Id);

            var currentStatus = status.First(x => x.Id == msg.Id);
            if (!currentStatus.IsActive)
                return false;

            var timePassed = (t - runningInteractions[name]).TotalMilliseconds;
            if (interactable.TimeToInteract - GameDesign.TimeTolerance > timePassed)
                return false;

            currentStatus.IsActive = false;
            RegisterStatusChange(currentStatus);

            ActorProxy.Create<IChatActor>(new ActorId(name)).WriteMessage(new ActorChatMessage()
            {
                Message = "You finished with " + msg.Id,
                Prefix = "System",
                Scope = ChatScope.System
            });

            return true;

            // TODO: Give Player Wood or sth
            // TODO: Check Player distance (Anti-Cheat)
        }

        private void RegisterStatusChange(InteractableStatusMessage currentStatus)
        {
            statusChanges.Add(DateTime.UtcNow, (InteractableStatusMessage) currentStatus.Clone());

            if (!currentStatus.IsActive)
            {
                var interactable = mapInfo.Interactables.First(x => x.Id == currentStatus.Id);

                respawnTime.Add(DateTime.UtcNow + TimeSpan.FromMilliseconds(interactable.RespawnTime), currentStatus.Id);
            }
        }

        public async Task<List<InteractableStatusMessage>> GetAllStatus()
        {
            return status;
        }

        public async Task<List<InteractableStatusMessage>> GetUpdateStatus(DateTime fromTime)
        {
            Cleanup();

            return statusChanges.Where(x => x.Key >= fromTime).Select(x => x.Value).ToList();
        }

        private void Cleanup()
        {
            var keys = statusChanges.Keys.ToList();

            foreach (DateTime key in keys)
            {
                if ((key - DateTime.UtcNow).TotalMilliseconds > GameDesign.ServerRememberEventTimeInMs)
                {
                    statusChanges.Remove(key);
                }
            }
        }
    }
}
