using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class PortScannerViewModel : ScannerViewModel<int>
    {
        public PortScannerViewModel()
        {
            IP = Dns.GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))?.ToString();
            From = IPEndPoint.MinPort;
            To = IPEndPoint.MaxPort;
        }

        private string _ip;
        public string IP
        {
            get { return _ip; }
            set { SetValue(ref _ip, value, nameof(IP)); }
        }

        protected override bool Validate(out string message)
        {
            message = null;
            if (!Validator.IsIP(IP))
            {
                message = "IP错误";
                return false;
            }
            if(!Validator.IsValidPort(From))
            {
                message = "起始端口超出范围";
                return false;
            }
            if(!Validator.IsValidPort(To))
            {
                message = "结束端口超出范围";
                return false;
            }
            return true;
        }

        protected override IEnumerable<int> Generate()
        {
            return Enumerable.Range(From, To - From + 1);
        }

        protected override async Task<bool> TestAsync(int item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var client = new TcpClient();
            using (var source = new CancellationTokenSource())
            {
                if (Timeout > 0)
                {
                    source.CancelAfter(Timeout);
                }
                try
                {
                    await client.ConnectAsync(IP, item, source.Token);
                }
                catch (SocketException)
                {
                    return false;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
                finally
                {
                    client.Close();
                }
                return true;
            }
        }
    }
}
