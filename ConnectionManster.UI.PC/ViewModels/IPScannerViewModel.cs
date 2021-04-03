using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class IPScannerViewModel : ScannerViewModel<string>
    {
        public IPScannerViewModel(PortScannerViewModel portScanner)
        {
            PortScanner = portScanner;
            portScanner.PropertyChanged += PortScanner_PropertyChanged;
            ScanPortCommand = new Command<string>(ScanPort,p=>!PortScanner.Scanning);
            SetDefaultIP();
        }

        private void PortScanner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(PortScanner.Scanning))
            {
                ScanPortCommand.OnCanExecuteChanged();
            }
        }

        public Command<string> ScanPortCommand { get; }

        public PortScannerViewModel PortScanner { get; }

        protected override bool Validate(out string message)
        {
            message = null;
            if(!Validator.IsIP(From))
            {
                message = "起始IP错误";
                return false;
            }
            if(!Validator.IsIP(To))
            {
                message = "结束IP错误";
                return false;
            }
            return true;
        }

        private void SetDefaultIP()
        {
            var localIP = Dns.GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));
            if (localIP == null)
            {
                return;
            }
            var prefix = Regex.Match(localIP.ToString(), @"^\d{1,3}(\.\d{1,3}){2}").Value;
            From = $"{prefix}.1";
            To = $"{prefix}.255";
        }

        private int IPToInt(string ip)
        {
            var values = ip.Split('.').Select(v => byte.Parse(v));
            int result = 0;
            foreach (var value in values)
            {
                result = (result << 8) | value;
            }
            return result;
        }

        private string IntToIP(int value)
        {
            var bytes = new byte[4];
            var last = bytes.Length - 1;
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[last - i] = (byte)((value >> (i << 3)) & 0xff);
            }
            return string.Join(".", bytes);
        }

        private void ScanPort(string ip)
        {
            PortScanner.IP = ip;
            if(PortScanner.Scanning)
            {
                return;
            }
            PortScanner.ScanCommand.Execute();
        }

        protected override IEnumerable<string> Generate()
        {
            var min = IPToInt(From);
            var max = IPToInt(To);
            return Enumerable.Range(min, max - min + 1)
                .Select(value => IntToIP(value));
        }

        protected override async Task<bool> TestAsync(string item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (var ping = new Ping())
            {
                var replay = Timeout > 0 ? await ping.SendPingAsync(item, Timeout) : await ping.SendPingAsync(item);
                return replay.Status == IPStatus.Success;
            }
        }
    }
}
