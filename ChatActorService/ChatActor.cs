using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ChatActorService.Interfaces;

namespace ChatActorService
{
    [StatePersistence(StatePersistence.Volatile)]
    public class ChatActor : Actor, IChatActor
    {
        private const string StateNameMessages = "Messages";

        /// <summary>
        /// Initializes a new instance of ChatActorService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public ChatActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override async Task OnActivateAsync()
        {
            await this.StateManager.TryAddStateAsync(StateNameMessages, new List<ActorChatMessage>());
            await base.OnActivateAsync();
        }

        protected override async Task OnDeactivateAsync()
        {
            await this.StateManager.RemoveStateAsync(StateNameMessages);
            await base.OnDeactivateAsync();
        }

        public async Task WriteMessage(ActorChatMessage msg)
        {
            var messages = await this.StateManager.GetStateAsync<List<ActorChatMessage>>(StateNameMessages);
            messages.Add(msg);

            await this.StateManager.SetStateAsync(StateNameMessages, messages);
            await this.StateManager.SaveStateAsync();
        }

        public async Task<List<ActorChatMessage>> GetMessages()
        {
            var messages = await this.StateManager.GetStateAsync<List<ActorChatMessage>>(StateNameMessages);
            await ClearMessages();
            await this.StateManager.SaveStateAsync();
            return messages;
        }

        private async Task ClearMessages()
        {
            await this.StateManager.SetStateAsync(StateNameMessages, new List<ActorChatMessage>());
        }
    }
}
