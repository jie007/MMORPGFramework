using System.Fabric;
using CommonServer.ServiceFabric;

namespace UdpChatService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public sealed class UdpChatService : UdpService
    {
        public UdpChatService(StatelessServiceContext context)
            : base(context, ServiceEventSource.Current.ServiceMessage, "ServiceEndpoint", new UdpListener())
        {

        }
    }
}
