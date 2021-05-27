using ConnectionMaster.Nat.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionMaster.Nat.Udp
{
    public class UdpNatClient : INatClient
    {
        private UdpClient udp;
        private ConcurrentDictionary<IPEndPoint, MemoryStream> buffers = new ConcurrentDictionary<IPEndPoint, MemoryStream>();

        public string Id { get; }

        public IPEndPoint ServerPoint { get; private set; }

        public UdpNatClient(string id,int port):this(id,new IPEndPoint(IPAddress.Loopback,port))
        {

        }

        public UdpNatClient(string id, IPEndPoint localPoint)
        {
            Id = id;
            LocalPoint = localPoint;
        }

        public IPEndPoint LocalPoint { get; }

        public int Timeout { get; set; }

        public bool IsOpened { get; private set; } = false;

        public async Task JoinAsync(IPEndPoint serverPoint, CancellationToken cancellationToken = default)
        {
            ServerPoint = serverPoint;
            await Send(new JoinMessage(Id), serverPoint,cancellationToken);
        }

        public async Task Send(NatMessage message,IPEndPoint clientPoint,CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = MessageTranslator.TranslateMessage(message);
            await udp.SendAsync(bytes, bytes.Length,clientPoint);
        }

        private async Task<T> ReceiveAsync<T>(IPEndPoint target) where T : NatMessage
        {
            var result = await udp.ReceiveAsync();
            if (!result.RemoteEndPoint.Equals(target))
            {
                return await ReceiveAsync<T>(target);
            }
            var stream = buffers.GetOrAdd(result.RemoteEndPoint, point => new MemoryStream());
            stream.Seek(0, SeekOrigin.End);
            await stream.WriteAsync(result.Buffer);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)await MessageTranslator.TranslateStreamAsync(stream);
        }

        public async Task OpenAsync()
        {
            if(IsOpened)
            {
                return;
            }
            udp = new UdpClient(LocalPoint);
            udp.AllowNatTraversal(true);
            IsOpened = true;
            await Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            if(!IsOpened)
            {
                return;
            }
            udp.Close();
            IsOpened = false;
            await Task.CompletedTask;
        }

        public async Task<IPEndPoint> FindClientAsync(string clientId, CancellationToken cancellationToken = default)
        {
            await Send(new FindClientMessage(clientId), ServerPoint, cancellationToken);
            var response = await ReceiveAsync<FindClientResponseMessage>(ServerPoint);
            return response.EndPoint;
        }

        public async Task ConnectClientAsync(IPEndPoint point, CancellationToken cancellationToken = default)
        {
            await Send(new ConnectClientMessage(Id), point, cancellationToken);
            await ReceiveAsync<ConnectClientMessage>(point);
        }

    }
}
