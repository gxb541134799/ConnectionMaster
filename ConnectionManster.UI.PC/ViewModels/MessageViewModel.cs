using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public abstract class MessageViewModel : ObservableObject
    {
        protected MessageViewModel()
        {
            Logger = new LoggerViewModel();
            SendCommand = new Command(Send, () => CanSend);
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetValue(ref _message, value, nameof(Message)); }
        }

        public LoggerViewModel Logger { get; }

        public int ReceiveTimeout { get; set; }

        public int ReceiveBufferSize { get; set; } = 1024;

        public bool ClearMessageAfterSend { get; set; } = false;

        public Command SendCommand { get; }

        public virtual bool CanSend
        {
            get
            {
                return !string.IsNullOrEmpty(Message);
            }
        }

        private async void Send()
        {
            try
            {
                await SendAsync();
            }
            catch (Exception ex)
            {
                Logger.Append($"发送失败：{ex.Message}");
                return;
            }
            if (ClearMessageAfterSend)
            {
                Message = null;
            }
        }

        protected abstract Task SendAsync();

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(Message))
            {
                SendCommand.OnCanExecuteChanged();
            }
        }
    }
}
