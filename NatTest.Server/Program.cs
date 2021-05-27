using ConnectionMaster.Nat;
using ConnectionMaster.Nat.Tcp;
using ConnectionMaster.Nat.Udp;
using System;
using System.Threading.Tasks;

namespace NatTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var protocal = args[0];
                var port = int.Parse(args[1]);
                INatServer server;
                switch(protocal.ToLower())
                {
                    case "tcp":
                        server = new TcpNatServer();
                        break;
                    case "udp":
                        server = new UdpNatServer();
                        break;
                    default:
                        Console.WriteLine("只支持TCP或UDP协议");
                        return;
                }
                server.ClientJoined += (s, e) =>
                  {
                      Console.WriteLine($"客户端{e.ClientId}({e.ClientPoint})上线");
                  };
                server.ClientLeaved += (s, e) =>
                  {
                      Console.WriteLine($"客户端{e.ClientId}({e.ClientPoint})离线");
                  };
                server.StartAsync(port);
                Console.WriteLine($"服务器已启动,协议:{protocal.ToUpper()},端口:{port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("输入quit退出");
            string line;
            while((line = Console.ReadLine()) != "quit")
            {

            }
        }
    }
}
