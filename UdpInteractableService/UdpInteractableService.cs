using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonServer.ServiceFabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace UdpInteractableService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class UdpInteractableService : UdpService
    {
        public UdpInteractableService(StatelessServiceContext context)
            : base(context, ServiceEventSource.Current.ServiceMessage, "ServiceEndpoint", new UdpListener())
        {

        }
    }
}
