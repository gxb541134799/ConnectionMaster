using ConnectionMaster.Nat;
using ConnectionMaster.Nat.Tcp;
using ConnectionMaster.Nat.Udp;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NatTest.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var protocal = args[0];
                var port = int.Parse(args[1]);
                var id = args[2];
                string serverIP = args[3];
                var serverPort = int.Parse(args[4]);
                INatClient client;
                switch(protocal.ToLower())
                {
                    case "tcp":
                        client = new TcpNatClient(id,port);
                        break;
                    case "udp":
                        client = new UdpNatClient(id, port);
                        break;
                    default:
                        Console.WriteLine("协议只支持TCP或UDP");
                        return;
                }
                await client.OpenAsync();
                Console.WriteLine("尝试连接服务器...");
                await client.JoinAsync(new IPEndPoint(IPAddress.Parse(serverIP), serverPort));
                Console.WriteLine("服务器已连接");
                Console.Write("请输入要尝试穿透的客户端Id：");
                var clientId = Console.ReadLine();
                Console.WriteLine("正在查找客户端...");
                var point = await client.FindClientAsync(clientId);
                if(point == null)
                {
                    Console.WriteLine("未找到客户端");
                    await client.CloseAsync();
                    return;
                }
                else
                {
                    Console.WriteLine($"客户端地址为:{point}");
                }
                Console.Write("请输入尝试连接次数：");
                var times = int.Parse(Console.ReadLine());
                while(times > 0)
                {
                    try
                    {
                        await client.ConnectClientAsync(point);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if(times -- > 0)
                        {
                            Console.WriteLine($"连接失败：{ex.Message},5秒后进行下一次尝试");
                        }
                        Thread.Sleep(5000);
                        continue;
                    }
                }
                if(times > 0)
                {
                    Console.WriteLine("穿透成功");
                }
                else
                {
                    Console.WriteLine("穿透失败");
                }
                await client.CloseAsync();
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
