using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonServer.ServiceFabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ReliableUdp;
using UdpMapService;

namespace UdpMapService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public sealed class UdpMapService : UdpService
    {
        public UdpMapService(StatelessServiceContext context)
            : base(context, ServiceEventSource.Current.ServiceMessage, "ServiceEndpoint", new UdpListener())
        {

        }
    }
}
