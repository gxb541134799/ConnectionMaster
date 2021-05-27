using ConnectionMaster.Nat.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionMaster.Nat
{
    public interface INatClient
    {
        string Id { get; }

        IPEndPoint LocalPoint { get; }

        bool IsOpened { get; }

        Task OpenAsync();

        Task CloseAsync();

        Task JoinAsync(IPEndPoint serverPoint, CancellationToken cancellationToken = default);

        Task<IPEndPoint> FindClientAsync(string clientId,CancellationToken cancellationToken = default);

        Task ConnectClientAsync(IPEndPoint point,CancellationToken cancellationToken = default);
    }
}
