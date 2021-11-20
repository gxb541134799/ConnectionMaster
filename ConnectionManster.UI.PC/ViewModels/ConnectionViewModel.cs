using ConnectionManster.UI.PC.Commands;
using ConnectionMaster.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public abstract class ConnectionViewModel : MessageViewModel
    {
        public IBinaryConnection Connection { get; protected set; }

        public AsyncCommand OpenCommand { get; }

        public AsyncCommand CloseCommand { get; }

        private bool _isOpened;
        public bool IsOpened
        {
            get { return _isOpened; }
            set { SetValue(ref _isOpened, value, nameof(IsOpened)); }
        }

        public override bool CanSend => IsOpened && base.CanSend;

        public FormatterViewModel FormatterViewModel { get; }

        protected ConnectionViewModel(FormatterViewModel formatterViewModel)
        {
            OpenCommand = new AsyncCommand(OpenAsync, () => !IsOpened);
            CloseCommand = new AsyncCommand(CloseAsync, () => IsOpened);
            FormatterViewModel = formatterViewModel;
        }

        protected override async Task SendCoreAsync()
        {
            var bytes = FormatterViewModel.Formatter.FromString(Message);
            await Connection.SendAsync(bytes, 0, bytes.Length);
        }

        private async Task OpenAsync()
        {
            string error;
            if(!Validate(out error))
            {
                Logger.Append(error);
                return;
            }
            Connection = CreateConnection();
            Connection.Closed += Connection_Closed;
            Connection.ReceiveBufferSize = ReceiveBufferSize;
            try
            {
                await Connection.OpenAsync();
            }
            catch (Exception ex)
            {
                Logger.Append($"连接失败：${ex.Message}");
                return;
            }
            OnOpenSuccess();
            IsOpened = true;
            BeginReceive();
        }

        private void Connection_Closed(object sender, EventArgs e)
        {
            OnClosed();
        }

        protected virtual void OnClosed()
        {
            IsOpened = false;
            Logger.Append("连接已关闭");
        }

        private async void BeginReceive()
        {
            while(IsOpened)
            {
                using var receiveTokenSource = new CancellationTokenSource();
                ReceiveResult result;
                if (ReceiveTimeout > 0)
                {
                    receiveTokenSource.CancelAfter(ReceiveTimeout);
                }
                try
                {
                    result = await Connection.ReceiveAsync(receiveTokenSource.Token);
                }
                catch (ObjectDisposedException)
                {
                    continue;
                }
                catch (OperationCanceledException)
                {
                    OnReceiveTimeout();
                    continue;
                }
                catch (Exception ex)
                {
                    OnReceiveFail(ex);
                    continue;
                }
                if (result.Data?.Length > 0)
                {
                    OnReceivedMessage(result);
                }
                else if(result.Data?.Length == 0)
                {
                    await CloseAsync();
                }
            }
        }

        private async Task CloseAsync()
        {
            if(!IsOpened)
            {
                return;
            }
            try
            {
                await Connection.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.Append($"断开连接异常：{ex.Message}");
                return;
            }
        }

        protected abstract IBinaryConnection CreateConnection();

        protected virtual void OnOpenSuccess()
        {
            Logger.Append("连接成功");
        }

        protected virtual void OnReceivedMessage(ReceiveResult result)
        {
            Logger.Append($"收到消息：{FormatterViewModel.Formatter.FromBytes(result.Data)}");
        }

        protected virtual void OnReceiveFail(Exception exception)
        {
            Logger.Append($"接收数据异常：{exception.Message}");
        }

        protected virtual void OnReceiveTimeout()
        {
            Logger.Append("接收超时");
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(IsOpened))
            {
                OpenCommand.OnCanExecuteChanged();
                CloseCommand.OnCanExecuteChanged();
                SendCommand.OnCanExecuteChanged();
            }
        }

        protected abstract bool Validate(out string message);
    }
}
