using ConnectionMaster.Nat.Messages;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConnectionMaster.Nat.Udp
{
    public class UdpNatServer : INatServer
    {
        private UdpClient udp;
        private ConcurrentDictionary<IPEndPoint, MemoryStream> buffers = new ConcurrentDictionary<IPEndPoint, MemoryStream>();

        public UdpNatServer()
        {
            Clients = new ConcurrentDictionary<string, IPEndPoint>();
        }

        public bool IsStopped { get; private set; } = true;

        public ConcurrentDictionary<string, IPEndPoint> Clients { get; }

        public event EventHandler<NatClientEventArgs> ClientJoined;

        public event EventHandler<NatClientEventArgs> ClientLeaved;

        public async Task StartAsync(int port)
        {
            if(!IsStopped)
            {
                return;
            }
            udp = new UdpClient(port);
            //udp.AllowNatTraversal(true);
            IsStopped = false;
            await ReceiveAsync();
        }

        public async Task StopAsync()
        {
            if(IsStopped)
            {
                return;
            }
            udp.Close();
            IsStopped = true;
            await Task.CompletedTask;
        }

        private async Task ReceiveAsync()
        {
            while(!IsStopped)
            {
                try
                {
                    var result = await udp.ReceiveAsync();
                    var stream = buffers.GetOrAdd(result.RemoteEndPoint, new MemoryStream());
                    stream.Seek(0, SeekOrigin.End);
                    stream.Write(result.Buffer);
                    stream.Seek(0, SeekOrigin.Begin);
                    var message = await MessageTranslator.TranslateStreamAsync(stream);
                    stream.Close();
                    buffers.TryRemove(result.RemoteEndPoint, out stream);
                    switch(message.MessageType)
                    {
                        case NatMessageType.Join:
                            var requestMessage = (JoinMessage)message;
                            Clients.AddOrUpdate(requestMessage.ClientId, id=>
                            {
                                ClientJoined?.Invoke(this, new NatClientEventArgs(requestMessage.ClientId, result.RemoteEndPoint));
                                return result.RemoteEndPoint;
                            }, (id, old) => result.RemoteEndPoint);
                            break;
                        case NatMessageType.FindClient:
                            var findClientMessage = (FindClientMessage)message;
                            Clients.TryGetValue(findClientMessage.ClientId, out IPEndPoint clientPoint);
                            await SendAsync(new FindClientResponseMessage(clientPoint),result.RemoteEndPoint);
                            break;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private Task SendAsync(NatMessage message,IPEndPoint target)
        {
            var bytes = MessageTranslator.TranslateMessage(message);
            return udp.SendAsync(bytes, bytes.Length, target);
        }
    }
}
