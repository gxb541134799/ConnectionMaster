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
        public IBinaryConnection Connection { get; private set; }

        public Command OpenCommand { get; }

        public Command CloseCommand { get; }

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
            OpenCommand = new Command(Open, () => !IsOpened);
            CloseCommand = new Command(Close, () => IsOpened);
            FormatterViewModel = formatterViewModel;
        }

        protected override async Task SendAsync()
        {
            var bytes = FormatterViewModel.Formatter.FromString(Message);
            await Connection.SendAsync(bytes, 0, bytes.Length);
        }

        private async void Open()
        {
            string error;
            if(!Validate(out error))
            {
                Logger.Append(error);
                return;
            }
            Connection = CreateConnection();
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

        private async void BeginReceive()
        {
            while(IsOpened)
            {
                ReceiveResult result;
                using(var source = new CancellationTokenSource())
                {
                    if(ReceiveTimeout > 0)
                    {
                        source.CancelAfter(ReceiveTimeout);
                    }
                    try
                    {
                        result = await Connection.ReceiveAsync(source.Token);
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
                }
            }
        }

        private async void Close()
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
            IsOpened = false;
            Logger.Append("连接已关闭");
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
