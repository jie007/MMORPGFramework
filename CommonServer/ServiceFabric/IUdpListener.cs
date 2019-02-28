using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliableUdp;

namespace CommonServer.ServiceFabric
{
    public interface IUdpListener: IUdpEventListener
    {
        void Update();
    }
}
