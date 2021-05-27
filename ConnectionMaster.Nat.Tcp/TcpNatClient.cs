using ConnectionMaster.Nat.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionMaster.Nat.Tcp
{
    public class TcpNatClient : INatClient
    {
        private Socket socket;
        private Socket listener;
        private NetworkStream stream;

        public string Id { get;}

        public IPEndPoint LocalPoint { get; }

        public bool IsOpened { get; private set; }

        public TcpNatClient(string id,int port):this(id,new IPEndPoint(IPAddress.Loopback,port))
        {

        }

        public TcpNatClient(string id,IPEndPoint localPoint)
        {
            Id = id;
            LocalPoint = localPoint;
        }

        public async Task<IPEndPoint> FindClientAsync(string clientId, CancellationToken cancellationToken = default)
        {
            await Send(new FindClientMessage(clientId));
            var response = await ReceiveAsync<FindClientResponseMessage>();
            return response.EndPoint;
        }

        public async Task ConnectClientAsync(IPEndPoint point, CancellationToken cancellationToken = default)
        {
            Disconnect();
            await Connect(point);
            await Send(new ConnectClientMessage(Id));
            await ReceiveAsync<ConnectClientMessage>();
        }

        public async Task JoinAsync(IPEndPoint serverPoint, CancellationToken cancellationToken = default)
        {
            await Connect(serverPoint);
            await Send(new JoinMessage(Id));
        }

        private async Task Connect(IPEndPoint remotePoint)
        {
            if (socket == null)
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.ExclusiveAddressUse = false;
                socket.Bind(LocalPoint);
            }
            if (!socket.Connected)
            {
                await socket.ConnectAsync(remotePoint);
                stream = new NetworkStream(socket);
            }
        }

        private void Disconnect()
        {
            if(socket == null)
            {
                return;
            }
            if(socket.Connected)
            {
                socket.Close();
            }
            socket = null;
        }

        public async Task Send(NatMessage message)
        {
            var bytes = MessageTranslator.TranslateMessage(message);
            await stream.WriteAsync(bytes);
        }

        private async Task<T> ReceiveAsync<T>() where T:NatMessage
        {
            return (T)await MessageTranslator.TranslateStreamAsync(stream);
        }

        public async Task OpenAsync()
        {
            if(IsOpened)
            {
                return;
            }
            listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.ExclusiveAddressUse = false;
            listener.Bind(new IPEndPoint(IPAddress.Any,LocalPoint.Port));
            listener.Listen(int.MaxValue);
            IsOpened = true;
            AcceptAsync();
            await Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            if(!IsOpened)
            {
                return;
            }
            listener.Close();
            IsOpened = false;
            await Task.CompletedTask;
        }

        private async void AcceptAsync()
        {
            await Task.Run(() =>
            {
                while(IsOpened)
                {
                    Socket newSocket;
                    try
                    {
                        newSocket = listener.Accept();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    ChatAsync(newSocket);
                }
            });
        }

        private async void ChatAsync(Socket socket)
        {
            var stream = new NetworkStream(socket);
            try
            {
                while (socket.Connected)
                {
                    var message = await MessageTranslator.TranslateStreamAsync(stream);
                    switch(message.MessageType)
                    {
                        case NatMessageType.ConnectClient:
                            var bytes = MessageTranslator.TranslateMessage(new ConnectClientMessage(Id));
                            await stream.WriteAsync(bytes);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                socket.Close();
            }
        }
    }
}
