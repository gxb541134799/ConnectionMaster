using ConnectionMaster.Core;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionMaster.SerialPort
{
    public class SerialPortConnection : IBinaryConnection
    {
        public System.IO.Ports.SerialPort Port { get; }

        public SerialPortConnection(System.IO.Ports.SerialPort port)
        {
            Port = port;
        }

        public SerialPortConnection() : this(new System.IO.Ports.SerialPort())
        {

        }

        public bool IsOpened => Port.IsOpen;

        public int ReceiveBufferSize { get; set; } = 1024;

        public int Delay { get; set; }

        public event EventHandler Closed;

        public Task CloseAsync()
        {
            if (IsOpened)
            {
                Port.Close();
                Closed?.Invoke(this, EventArgs.Empty);
            }
            return Task.CompletedTask;
        }

        public Task OpenAsync()
        {
            if (!IsOpened)
            {
                Port.Open();
            }
            return Task.CompletedTask;
        }

        public async Task<ReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            EnsureIsOpened();
            if (Delay > 0)
            {
                await Task.Delay(Delay);
            }
            var buffer = new Memory<byte>(new byte[ReceiveBufferSize]);
            var length =await Port.BaseStream.ReadAsync(buffer,cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = buffer.Slice(0, length).ToArray();
            return new ReceiveResult(bytes);
        }

        public async Task SendAsync(byte[] message, int startIndex, int count,CancellationToken cancellationToken = default)
        {
            EnsureIsOpened();
            await Port.BaseStream.WriteAsync(message, startIndex, count,cancellationToken);
        }

        private void EnsureIsOpened()
        {
            if (!IsOpened)
            {
                throw new InvalidOperationException("???????????????");
            }
        }
    }
}
