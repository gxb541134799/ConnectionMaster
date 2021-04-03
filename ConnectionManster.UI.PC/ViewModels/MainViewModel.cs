using ConnectionManster.UI.PC.Formatters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class MainViewModel
    {
        public TcpConnectionViewModel TcpClient { get; }

        public TcpServerViewModel TcpServer { get; }

        public UdpConnectionViewModel Udp { get; }

        public SerialPortConnectionViewModel SerialPort { get; }

        public HttpViewModel Http { get; }

        public ByteConvertViewModel Converter { get; }

        public IPScannerViewModel IPScanner { get; }

        public PortScannerViewModel PortScanner { get; }

        public SymmetricViewModel Symmetric { get; }

        public MainViewModel()
        {
            var services = new ServiceCollection();
            ConfigServices(services);
            var provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();
            var formatters = scope.ServiceProvider.GetServices<IMessageFormatter>();
            TcpServer = new TcpServerViewModel(new FormatterViewModel(formatters));
            TcpClient = new TcpConnectionViewModel(new FormatterViewModel(formatters));
            Udp = new UdpConnectionViewModel(new FormatterViewModel(formatters));
            SerialPort = new SerialPortConnectionViewModel(new FormatterViewModel(formatters));
            Http = new HttpViewModel(new FormatterViewModel(formatters));
            Converter = new ByteConvertViewModel(new FormatterViewModel(formatters));
            PortScanner = new PortScannerViewModel();
            IPScanner = new IPScannerViewModel(PortScanner);
            Symmetric = new SymmetricViewModel(new FormatterViewModel(formatters));
        }

        private void ConfigServices(IServiceCollection services)
        {
            services.AddSingleton<IMessageFormatter, TextMessageFormatter>();
            services.AddSingleton<IMessageFormatter, BinaryMessageFormatter>();
            services.AddSingleton<IMessageFormatter, Base64Formatter>();
        }
    }
}
