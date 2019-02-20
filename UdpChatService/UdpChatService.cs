using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ReliableUdp;

namespace UdpChatService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public sealed class UdpChatService : StatelessService
    {
        private UdpManagerListener listener;

        public UdpChatService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            FactoryRegistrations.Register();

            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                {
                    listener = new UdpManagerListener(serviceContext, ServiceEventSource.Current, "ServiceEndpoint");
                    return this.listener;
                })
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                this.listener.Update();

                Task.Delay(50).Wait();
            }

            return base.RunAsync(cancellationToken);
        }
    }
}
