using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ReliableUdp;

namespace CommonServer.ServiceFabric
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public class UdpService : StatelessService
    {
        private UdpManagerListener listener;

        private string endpointName;
        private readonly IUdpListener udpListener;
        private readonly Action<StatelessServiceContext, string> serviceMessage;

        public UdpService(StatelessServiceContext context, Action<StatelessServiceContext, string> serviceMessage, string endpointName, IUdpListener udpListener)
            : base(context)
        {
            this.endpointName = endpointName;
            this.udpListener = udpListener;
            this.serviceMessage = serviceMessage;
        }

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
                    listener = new UdpManagerListener(serviceContext, this.serviceMessage, this.endpointName, this.udpListener);
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
