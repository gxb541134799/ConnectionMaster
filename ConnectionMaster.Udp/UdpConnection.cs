using ConnectionMaster.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionMaster.Udp
{
    public class UdpConnection : IBinaryConnection
    {
        private readonly UdpClient client;

        public UdpConnection(UdpClient client)
        {
            this.client = client;
            Receivers = new LinkedList<IPEndPoint>();
        }

        public UdpConnection(int port):this(new UdpClient(port))
        {
            
        }

        public bool IsOpened { get; private set; } = false;

        public int ReceiveBufferSize { get; set; } = 1024;

        public ICollection<IPEndPoint> Receivers { get; }

        public Task CloseAsync()
        {
            if(IsOpened)
            {
                client.Close();
                IsOpened = false;
            }
            return Task.CompletedTask;
        }

        public Task OpenAsync()
        {
            if(!IsOpened)
            {
                IsOpened = true;
            }
            return Task.CompletedTask;
        }

        public async Task<ReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            EnsureIsOpened();
            var result = await client.ReceiveAsync();
            return new ReceiveResult(result.Buffer, result.RemoteEndPoint);
        }

        public Task SendAsync(byte[] message, int startIndex, int count,CancellationToken cancellationToken = default)
        {
            EnsureIsOpened();
            var bytes = new Memory<byte>(message, startIndex, count).ToArray();
            return Task.Run(async () =>
            {
                foreach (var receiver in Receivers)
                {
                    await client.SendAsync(bytes, bytes.Length, receiver);
                }
            }, cancellationToken);
            
        }

        private void EnsureIsOpened()
        {
            if(!IsOpened)
            {
                throw new InvalidOperationException("未打开连接");
            }
        }
    }
}
