using ConnectionManster.UI.PC.Commands;
using ConnectionMaster.Core;
using ConnectionMaster.SerialPort;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class SerialPortConnectionViewModel : ConnectionViewModel
    {
        public ObservableCollection<string> PortNames { get; }

        private string _portName;
        public string PortName
        {
            get { return _portName; }
            set { SetValue(ref _portName, value, nameof(PortName)); }
        }

        public int BaudRate { get; set; } = 9600;

        public int[] BaudRateOptions { get; }

        public StopBits StopBits { get; set; } = StopBits.One;

        private Parity _parity = Parity.None;
        public Parity Parity
        {
            get { return _parity; }
            set { SetValue(ref _parity, value, nameof(Parity)); }
        }

        public byte ParityReplace { get; set; }

        public int[] DataBitsOptions { get; }

        public int DataBits { get; set; } = 8;

        public bool DiscardNull { get; set; } = true;

        public bool DtrEnable { get; set; } = false;

        public bool RtsEnable { get; set; } = false;

        public Handshake Handshake { get; set; }

        public int Delay { get; set; } = 100;

        public Command RefreshCommand { get; }

        public SerialPortConnectionViewModel(FormatterViewModel formatterViewModel) : base(formatterViewModel)
        {
            DataBitsOptions = Enumerable.Range(5, 4).OrderByDescending(i=>i).ToArray();
            PortNames = new ObservableCollection<string>();
            RefreshCommand = new Command(Refresh, () => !IsOpened);
            RefreshCommand.Execute();
            BaudRateOptions = new int[]
            {
                75,110,134,150,300,600,
                1200,1800,2400,4800,7200,9600,
                14400,19200,38400,57600,115200,128000
            };
        }

        protected override IBinaryConnection CreateConnection()
        {
            var port = new SerialPort(PortName)
            {
                BaudRate = BaudRate,
                StopBits = StopBits,
                Parity = Parity,
                ParityReplace = ParityReplace,
                DataBits = DataBits,
                DiscardNull = DiscardNull,
                DtrEnable = DtrEnable,
                Handshake = Handshake,
                RtsEnable = RtsEnable
            };
            return new SerialPortConnection(port)
            {
                Delay = Delay
            };
        }

        protected override bool Validate(out string message)
        {
            if(string.IsNullOrEmpty(PortName))
            {
                message = "请选择串口";
                return false;
            }
            message = null;
            return true;
        }

        private void Refresh()
        {
            PortNames.Clear();
            foreach(var name in SerialPort.GetPortNames())
            {
                PortNames.Add(name);
            }
            if(PortName == null)
            {
                PortName = PortNames.FirstOrDefault();
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(IsOpened))
            {
                RefreshCommand.OnCanExecuteChanged();
            }
        }
    }
}
