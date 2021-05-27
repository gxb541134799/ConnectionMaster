using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace ConnectionMaster.Nat
{
    public interface INatServer
    {
        bool IsStopped { get; }

        ConcurrentDictionary<string,IPEndPoint> Clients { get; }

        event EventHandler<NatClientEventArgs> ClientJoined;

        event EventHandler<NatClientEventArgs> ClientLeaved;

        Task StartAsync(int port);

        Task StopAsync();
    }
}
