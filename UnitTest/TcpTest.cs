using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class TcpTest
    {
        [TestMethod]
        public async Task Cancel()
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, 7900);
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(5000);
            var buffer = new Memory<byte>(new byte[100]);
            try
            {
                int length = await client.GetStream().ReadAsync(buffer, tokenSource.Token);
                Console.WriteLine(length);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("接收已取消");
            }
        }
    }
}
