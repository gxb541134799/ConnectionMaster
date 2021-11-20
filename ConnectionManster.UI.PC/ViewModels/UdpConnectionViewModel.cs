using ConnectionManster.UI.PC.Commands;
using ConnectionMaster.Core;
using ConnectionMaster.Udp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class UdpConnectionViewModel : ConnectionViewModel
    {
        public int Port { get; set; }

        public ObservableCollection<Checkable<IPEndPoint>> Receivers { get; }

        public IEnumerable<IPEndPoint> SelectedPoints
        {
            get
            {
                return Receivers.Where(r => r.IsChecked).Select(r => r.Value);
            }
        }

        public Command AddReceiverCommand { get; }

        public Command<IPEndPoint> RemoveReceiverCommand { get; }

        public UdpConnectionViewModel(FormatterViewModel formatterViewModel) : base(formatterViewModel)
        {
            Receivers = new ObservableCollection<Checkable<IPEndPoint>>();
            AddReceiverCommand = new Command(AddReceiver);
            RemoveReceiverCommand = new Command<IPEndPoint>(RemoveReciever,p=>p != null);
        }

        protected override IBinaryConnection CreateConnection()
        {
            return new UdpConnection(Port);
        }

        protected override void OnOpenSuccess()
        {
            Logger.Append($"开始监听{Port}端口");
        }

        protected override void OnReceivedMessage(ReceiveResult result)
        {
            var point = (IPEndPoint)result.Payload;
            Logger.Append($"收到{point}的消息:{FormatterViewModel.Formatter.FromBytes(result.Data)}");
            App.Current.Dispatcher.Invoke(() =>
            {
                AddReceiver(point);
            });
        }

        protected override async Task SendCoreAsync()
        {
            var udpConnection = (UdpConnection)Connection;
            udpConnection.Receivers.Clear();
            foreach(var point in SelectedPoints)
            {
                udpConnection.Receivers.Add(point);
            }
            await base.SendCoreAsync();
            foreach(var point in udpConnection.Receivers)
            {
                Logger.Append($"向{point}发送消息:{Message}");
            }
        }

        protected override bool Validate(out string message)
        {
            if(!Validator.IsValidPort(Port))
            {
                message = $"端口不能超出范围：0~{ushort.MaxValue}";
                return false;
            }
            message = null;
            return true;
        }

        private void AddReceiver()
        {
            var viewModel = new IPEndPointViewModel();
            viewModel.SaveSuccess += (s, e) =>
              {
                  AddReceiver(e);
              };
            Notify.ShowViewModel(viewModel);
        }

        private void AddReceiver(IPEndPoint point)
        {
            if(!Receivers.Any(e=>e.Value.Equals(point)))
            {
                Receivers.Add(new Checkable<IPEndPoint>(point,true));
            }
        }

        private void RemoveReciever(IPEndPoint point)
        {
            Receivers.Remove(Receivers.Single(r => r.Value.Equals(point)));
        }
    }
}
