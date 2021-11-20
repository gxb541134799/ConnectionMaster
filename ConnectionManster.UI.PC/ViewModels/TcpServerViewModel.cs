using ConnectionManster.UI.PC.Commands;
using ConnectionMaster.Core;
using ConnectionMaster.Tcp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class TcpServerViewModel : MessageViewModel
    {
        private TcpListener listener;

        public TcpServerViewModel(FormatterViewModel formatterViewModel)
        {
            FormatterViewModel = formatterViewModel;
            Clients = new ObservableCollection<Checkable<TcpConnection>>();
            StartCommand = new AsyncCommand(StartAsync, () => IsStopped);
            StopCommand = new AsyncCommand(StopAsync, () => !IsStopped);
        }

        public int Port { get; set; }

        private bool _isStopeed = true;
        public bool IsStopped
        {
            get { return _isStopeed; }
            set { SetValue(ref _isStopeed, value, nameof(IsStopped)); }
        }

        public ObservableCollection<Checkable<TcpConnection>> Clients { get; }

        public IEnumerable<TcpConnection> SelectedClients
        {
            get
            {
                return Clients.Where(c => c.IsChecked).Select(c =>c.Value);
            }
        }

        public FormatterViewModel FormatterViewModel { get; }

        public AsyncCommand StartCommand { get; }

        public AsyncCommand StopCommand { get; }

        private async Task StartAsync()
        {
            if(!Validator.IsValidPort(Port))
            {
                Logger.Append($"端口不能超出范围：0~{ushort.MaxValue}");
                return;
            }
            try
            {
                listener = new TcpListener(IPAddress.Any, Port);
                listener.Start();
            }
            catch (Exception ex)
            {
                Logger.Append($"启动失败：{ex.Message}");
                return;
            }
            IsStopped = false;
            Logger.Append($"开始监听{Port}端口");
            await BeginAccept();
        }

        private async Task StopAsync()
        {
            while(Clients.Count > 0)
            {
                await Clients[0].Value.CloseAsync();
            }
            try
            {
                listener.Stop();
            }
            catch (Exception ex)
            {
                Logger.Append($"停止失败：{ex.Message}");
                return;
            }
            IsStopped = true;
            Logger.Append("端口监听已停止");
        }

        private async Task BeginAccept()
        {
            while (!IsStopped)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync();
                }
                catch(SocketException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    Logger.Append($"接受客户端异常：{ex.Message}");
                    return;
                }
                var connection = new TcpConnection(client)
                {
                    ReceiveBufferSize = ReceiveBufferSize
                };
                connection.Closed += (sender, e) => OnConnectionClosed((TcpConnection)sender);
                Clients.Add(new Checkable<TcpConnection>(connection, true));
                Logger.Append($"{client.Client.RemoteEndPoint}已连接");
                BeginReceive(connection);
            }
        }

        private async void BeginReceive(TcpConnection connection)
        {
            while (connection.IsOpened)
            {
                var receiveTokenSource = new CancellationTokenSource();
                if (ReceiveTimeout > 0)
                {
                    receiveTokenSource.CancelAfter(ReceiveTimeout);
                }
                ReceiveResult result;
                try
                {
                    result = await connection.ReceiveAsync(receiveTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Logger.Append($"接收{connection.RemotePoint}的消息超时");
                    break;
                }
                catch (Exception)
                {
                    break;
                }
                if (result.Data.Length == 0)
                {
                    break;
                }
                Logger.Append($"收到来自{connection.RemotePoint}的消息:{FormatterViewModel.Formatter.FromBytes(result.Data)}");
            }
            await connection.CloseAsync();
        }

        protected override async Task SendCoreAsync()
        {
            var selectedClients = SelectedClients.ToArray();
            var bytes = FormatterViewModel.Formatter.FromString(Message);
            foreach (var client in selectedClients)
            {
                await client.SendAsync(bytes, 0, bytes.Length);
                Logger.Append($"向{client.RemotePoint}发送数据：{Message}");
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(IsStopped))
            {
                StartCommand.OnCanExecuteChanged();
                StopCommand.OnCanExecuteChanged();
            }
        }

        private void OnConnectionClosed(TcpConnection connection)
        {
            Logger.Append($"与{connection.RemotePoint}断开了连接");
            App.Current.Dispatcher.Invoke(() =>
            {
                Clients.Remove(Clients.Single(c => c.Value == connection));
            });
        }
    }
}
