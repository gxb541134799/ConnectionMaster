using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class IPEndPointViewModel:ObservableObject
    {
        private string _ip;
        public string IP
        {
            get { return _ip; }
            set { SetValue(ref _ip, value, nameof(IP)); }
        }

        public int Port { get; set; }

        public Command SaveCommand { get; }

        public event EventHandler<IPEndPoint> SaveSuccess;

        public IPEndPointViewModel()
        {
            SaveCommand = new Command(Save, () => !string.IsNullOrEmpty(IP));
        }

        private void Save()
        {
            if(!Validator.IsIP(IP))
            {
                Notify.ShowError("IP地址错误", "校验");
                return;
            }
            if(!Validator.IsValidPort(Port))
            {
                Notify.ShowError($"端口号不能超出范围：{IPEndPoint.MinPort}~{IPEndPoint.MaxPort}", "校验");
                return;
            }
            var result = new IPEndPoint(IPAddress.Parse(IP), Port);
            SaveSuccess?.Invoke(this, result);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(IP))
            {
                SaveCommand.OnCanExecuteChanged();
            }
        }
    }
}
