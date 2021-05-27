using ConnectionMaster.Nat.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionMaster.Nat.Tcp
{
    public class TcpNatServer : INatServer
    {
        private Socket socket;

        public TcpNatServer()
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
            socket = new Socket(SocketType.Stream,ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(int.MaxValue);
            IsStopped = false;
            BeginAcceptClient();
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if(IsStopped)
            {
                return;
            }
            socket.Close();
            IsStopped = true;
            await Task.CompletedTask;
        }

        private void BeginAcceptClient()
        {
            Task.Run(() =>
            {
                Socket newSocket;
                while (!IsStopped)
                {
                    try
                    {
                        newSocket = socket.Accept();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    Task.Run(()=>ChatAsync(newSocket));
                }
            });
        }

        private async void ChatAsync(Socket socket)
        {
            if(!socket.Connected)
            {
                return;
            }
            var localPoint = (IPEndPoint)socket.RemoteEndPoint;
            var stream = new NetworkStream(socket);
            string clientId = null;
            try
            {
                while (socket.Connected)
                {
                    var message =await MessageTranslator.TranslateStreamAsync(stream);
                    switch(message.MessageType)
                    {
                        case NatMessageType.Join:
                            var joinRequestMessage = (JoinMessage)message;
                            clientId = joinRequestMessage.ClientId;
                            Clients.AddOrUpdate(clientId, id=>
                            {
                                ClientJoined?.Invoke(this, new NatClientEventArgs(clientId,localPoint));
                                return localPoint;
                            },(id,old)=>localPoint);
                            break;
                        case NatMessageType.FindClient:
                            var findClientMessage = (FindClientMessage)message;
                            Clients.TryGetValue(findClientMessage.ClientId, out IPEndPoint clientPoint);
                            var response = MessageTranslator.TranslateMessage(new FindClientResponseMessage(clientPoint));
                            await stream.WriteAsync(response);
                            break;
                        default:
                            socket.Close();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                socket.Close();
                if(clientId != null && Clients.TryRemove(clientId,out localPoint))
                {
                    ClientLeaved?.Invoke(this, new NatClientEventArgs(clientId,localPoint));
                }
            }
            
        }
    }
}
