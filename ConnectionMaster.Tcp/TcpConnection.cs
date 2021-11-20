using ConnectionMaster.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionMaster.Tcp
{
    public sealed class TcpConnection : IBinaryConnection
    {
        private readonly TcpClient client;

        public event EventHandler Closed;

        public TcpConnection(IPEndPoint remotePoint)
        {
            RemotePoint = remotePoint;
            client = new TcpClient();
        }

        public TcpConnection(IPAddress ip,int port):this(new IPEndPoint(ip,port))
        {

        }

        public TcpConnection(string ip,int port):this(IPAddress.Parse(ip),port)
        {

        }

        public TcpConnection(TcpClient client)
        {
            if(!client.Connected)
            {
                throw new NotSupportedException("不支持未连接的client");
            }
            this.client = client;
            this.IsOpened = true;
            RemotePoint = (IPEndPoint)client.Client.RemoteEndPoint;
        }

        public TcpConnection(Socket socket)
        {
            if(!socket.Connected)
            {
                throw new NotSupportedException("不支持未连接的socket");
            }
            this.client = new TcpClient();
            this.client.Client = socket;
            IsOpened = true;
            RemotePoint = (IPEndPoint)socket.RemoteEndPoint;
        }

        public void Bind(IPEndPoint localPoint)
        {
            if(this.IsOpened)
            {
                throw new InvalidOperationException("连接后不能绑定本地节点");
            }
            this.client.Client.Bind(localPoint);
        }

        public bool IsOpened { get; private set; } = false;

        public IPEndPoint RemotePoint { get; }

        public int ReceiveBufferSize { get; set; } = 1024;

        public Task CloseAsync()
        {
            if(IsOpened)
            {
                client.Close();
                IsOpened = false;
                Closed?.Invoke(this, EventArgs.Empty);
            }
            return Task.CompletedTask;
        }

        public async Task OpenAsync()
        {
            if(IsOpened)
            {
                return;
            }
            await client.ConnectAsync(RemotePoint.Address,RemotePoint.Port);
            IsOpened = true;
        }

        public async Task<ReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            EnsureIsOpened();
            var stream = client.GetStream();
            var buffer = new Memory<byte>(new byte[ReceiveBufferSize]);
            int length = await stream.ReadAsync(buffer, cancellationToken);
            var bytes = buffer.Slice(0, length).ToArray();
            return new ReceiveResult(bytes);
        }

        public async Task SendAsync(byte[] message,int startIndex,int count,CancellationToken cancellationToken=default)
        {
            EnsureIsOpened();
            await client.GetStream().WriteAsync(new Memory<byte>(message,startIndex,count),cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private void EnsureIsOpened()
        {
            if (!IsOpened)
            {
                throw new InvalidOperationException("连接未打开");
            }
        }
    }
}
