using ConnectionMaster.Core;
using ConnectionMaster.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class TcpConnectionViewModel : ConnectionViewModel
    {
        public TcpConnectionViewModel(FormatterViewModel formatterViewModel) : base(formatterViewModel)
        {
            IPOrHost = Dns.GetHostAddresses("localhost")
                .FirstOrDefault(ip=>ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }

        public string IPOrHost { get; set; }

        public int Port { get; set; }

        protected override IBinaryConnection CreateConnection()
        {
            if (Validator.IsIP(IPOrHost))
            {
                return new TcpConnection(IPOrHost, Port);
            }
            return new TcpConnection(Dns.GetHostAddresses(IPOrHost)[0], Port);
        }

        protected override async Task SendCoreAsync()
        {
            await base.SendCoreAsync();
            Logger.Append($"发送数据:{Message}");
        }

        protected override void OnOpenSuccess()
        {
            var connection = (TcpConnection)Connection;
            Logger.Append($"已连接至{connection.RemotePoint}");
        }

        protected override void OnReceiveFail(Exception exception)
        {
            CloseCommand.Execute();
            if(exception is IOException)
            {
                return;
            }
            base.OnReceiveFail(exception);
        }

        protected override void OnReceiveTimeout()
        {
            base.OnReceiveTimeout();
            CloseCommand.Execute();
        }

        protected override bool Validate(out string message)
        {
            if (string.IsNullOrEmpty(IPOrHost))
            {
                message = "请填写IP地址或主机名";
                return false;
            }
            if (!Validator.IsValidPort(Port))
            {
                message = $"端口不能超出范围0~{ushort.MaxValue}";
                return false;
            }
            message = null;
            return true;
        }
    }
}
