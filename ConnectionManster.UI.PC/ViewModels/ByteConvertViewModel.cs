using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class ByteConvertViewModel:ObservableObject
    {
        public ByteConvertViewModel(FormatterViewModel formatterViewModel)
        {
            FormatterViewModel = formatterViewModel;
            Func<bool> canConvert = () => !string.IsNullOrEmpty(Input);
            SwitchCommand = new Command(Switch);
            ToInt16Command = new Command(ToInt16,canConvert);
            ToUInt16Command = new Command(ToUInt16, canConvert);
            ToInt32Command = new Command(ToInt32,canConvert);
            ToUInt32Command = new Command(ToUInt32, canConvert);
            ToInt64Command = new Command(ToInt64, canConvert);
            ToUInt64Command = new Command(ToUInt64, canConvert);
            ToFloatCommand = new Command(ToFloat, canConvert);
            ToDoubleCommand = new Command(ToDouble, canConvert);
            ToBase64Command = new Command(ToBase64, canConvert);
            ToMD5Command = new Command(ToMD5, canConvert);
            ToSHA1Command = new Command(ToSHA1, canConvert);
            ToCRCCommand = new Command(ToCRC, canConvert);

            Int16ToBytesCommand = new Command(Int16ToBytes, canConvert);
            UInt16ToBytesCommand = new Command(UInt16ToBytes, canConvert);
            Int32ToBytesCommand = new Command(Int32ToBytes, canConvert);
            UInt32ToBytesCommand = new Command(UInt32ToBytes, canConvert);
            Int64ToBytesCommand = new Command(Int64ToBytes, canConvert);
            UInt64ToBytesCommand = new Command(UInt64ToBytes, canConvert);
            FloatToBytesCommand = new Command(FloatToBytes, canConvert);
            DoubleToBytesCommand = new Command(DoubleToBytes, canConvert);
            FromBase64Command = new Command(FromBase64, canConvert);
        }

        private string _input;
        public string Input
        {
            get { return _input; }
            set { SetValue(ref _input, value, nameof(Input)); }
        }

        private string _output;
        public string Output
        {
            get { return _output; }
            set { SetValue(ref _output,value,nameof(Output)); }
        }

        public bool IsLittleEndian { get; set; } = false;

        public FormatterViewModel FormatterViewModel { get; }

        #region 命令
        public Command SwitchCommand { get; }

        public Command ToInt16Command { get; }

        public Command ToUInt16Command { get; }

        public Command ToInt32Command { get; }

        public Command ToUInt32Command { get; }

        public Command ToInt64Command { get;}

        public Command ToUInt64Command { get; }

        public Command ToFloatCommand { get; }

        public Command ToDoubleCommand { get; }

        public Command Int16ToBytesCommand { get; }

        public Command UInt16ToBytesCommand { get; }

        public Command Int32ToBytesCommand { get; }

        public Command UInt32ToBytesCommand { get; }

        public Command Int64ToBytesCommand { get; }

        public Command UInt64ToBytesCommand { get; }

        public Command FloatToBytesCommand { get; }

        public Command DoubleToBytesCommand { get; }

        public Command ToBase64Command { get; }

        public Command FromBase64Command { get; }

        public Command ToSHA1Command { get; }

        public Command ToMD5Command { get; }

        public Command ToCRCCommand { get; }
        #endregion

        #region 方法
        private void Switch()
        {
            var temp = Input;
            Input = Output;
            Output = temp;
        }

        private void ToInt16()
        {
            TryConvertNumber(2, bytes => BitConverter.ToInt16(bytes));
        }

        private void ToUInt16()
        {
            TryConvertNumber(2, bytes => BitConverter.ToUInt16(bytes));
        }

        private void ToInt32()
        {
            TryConvertNumber(4, bytes => BitConverter.ToInt32(bytes));
        }

        private void ToUInt32()
        {
            TryConvertNumber(4, bytes => BitConverter.ToUInt32(bytes));
        }

        private void ToInt64()
        {
            TryConvertNumber(8, bytes => BitConverter.ToInt64(bytes));
        }

        private void ToUInt64()
        {
            TryConvertNumber(8, bytes => BitConverter.ToUInt64(bytes));
        }

        private void ToFloat()
        {
            TryConvertNumber(4, bytes => (decimal)BitConverter.ToSingle(bytes));
        }

        private void ToDouble()
        {
            TryConvertNumber(8, bytes => (decimal)BitConverter.ToDouble(bytes));
        }

        private void Int16ToBytes()
        {
            TryConvertText(text=>BitConverter.GetBytes(short.Parse(text)));
        }

        private void UInt16ToBytes()
        {
            TryConvertText(text => BitConverter.GetBytes(ushort.Parse(text)));
        }

        private void Int32ToBytes()
        {
            TryConvertText(text => BitConverter.GetBytes(int.Parse(text)));
        }

        private void UInt32ToBytes()
        {
            TryConvertText(text => BitConverter.GetBytes(uint.Parse(text)));
        }

        private void Int64ToBytes()
        {
            TryConvertText(text => BitConverter.GetBytes(long.Parse(text)));
        }

        private void UInt64ToBytes()
        {
            TryConvertText(text => BitConverter.GetBytes(ulong.Parse(text)));
        }

        private void FloatToBytes()
        {
            TryConvertText(text => BitConverter.GetBytes(float.Parse(text)));
        }

        private void DoubleToBytes()
        {
            TryConvertText(text => BitConverter.GetBytes(double.Parse(text)));
        }

        private void FromBase64()
        {
            try
            {
                var bytes = Convert.FromBase64String(Input);
                Output = FormatterViewModel.Formatter.FromBytes(bytes);
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "转换失败");
            }
            
        }

        private void ToBase64()
        {
            try
            {
                var bytes = FormatterViewModel.Formatter.FromString(Input);
                Output = Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "转换失败");
            }
            
        }

        private void ToMD5()
        {
            using(var md5 = MD5.Create())
            {
                TryConvertHash(md5);
            }
        }

        private void ToSHA1()
        {
            using(var sha1 = SHA1.Create())
            {
                TryConvertHash(sha1);
            }
        }

        private void ToCRC()
        {
            try
            {
                var bytes = FormatterViewModel.Formatter.FromString(Input);
                var crc = ToCRC(bytes);
                bytes = GetFinalBytes(BitConverter.GetBytes(crc));
                Output = ToHexString(bytes);
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "转换失败");
            }
        }

        private ushort ToCRC(byte[] input)
        {
            int len = input.Length;
            ushort crc = 0xFFFF;
            for (int i = 0; i < len; i++)
            {
                crc = (ushort)(crc ^ input[i]);
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)(crc >> 1 ^ 0xA001) : (ushort)(crc >> 1);
                }
            }
            return crc;
        }

        private void TryConvertHash(HashAlgorithm hash)
        {
            var formatter = FormatterViewModel.Formatter;
            try
            {
                var inputBytes = formatter.FromString(Input);
                var hashBytes = hash.ComputeHash(inputBytes);
                Output = ToHexString(hashBytes,"");
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "转换失败");
            }
        }

        private void TryConvertNumber(int length,Func<byte[],decimal> converter)
        {
            TryConvertBytes(bytes =>
            {
                if(bytes.Length > length)
                {
                    Notify.ShowError($"字节长度必须为{length}","转换失败");
                }
                return converter(bytes).ToString();
            });
        }

        private void TryConvertBytes(Func<byte[],string> converter)
        {
            try
            {
                var bytes = GetFinalBytes(Input);
                Output = converter(bytes);
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "转换失败");
            }
        }

        private void TryConvertText(Func<string,byte[]> converter)
        {
            try
            {
                var bytes = converter(Input);
                Output = ToHexString(GetFinalBytes(bytes));
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "转换失败");
            }
        }

        private byte[] FromHexString(string text)
        {
            return text.Split(' ')
                .Select(value => byte.Parse(value, System.Globalization.NumberStyles.HexNumber))
                .ToArray();
        }

        private string ToHexString(IEnumerable<byte> bytes,string seperator = " ")
        {
            return string.Join(seperator, bytes.Select(b => b.ToString("X2")));
        }

        private byte[] GetFinalBytes(byte[] originBytes)
        {
            if(BitConverter.IsLittleEndian == IsLittleEndian)
            {
                return originBytes;
            }
            return originBytes.Reverse().ToArray();
        }

        private byte[] GetFinalBytes(string hexString)
        {
            var bytes = FromHexString(hexString);
            return GetFinalBytes(bytes);
        }
        #endregion

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(Input))
            {
                ToInt16Command.OnCanExecuteChanged();
                ToUInt16Command.OnCanExecuteChanged();
                ToInt32Command.OnCanExecuteChanged();
                ToUInt32Command.OnCanExecuteChanged();
                ToInt64Command.OnCanExecuteChanged();
                ToUInt64Command.OnCanExecuteChanged();
                ToFloatCommand.OnCanExecuteChanged();
                ToDoubleCommand.OnCanExecuteChanged();
                ToBase64Command.OnCanExecuteChanged();
                ToMD5Command.OnCanExecuteChanged();
                ToSHA1Command.OnCanExecuteChanged();
                ToCRCCommand.OnCanExecuteChanged();

                Int16ToBytesCommand.OnCanExecuteChanged();
                UInt16ToBytesCommand.OnCanExecuteChanged();
                Int32ToBytesCommand.OnCanExecuteChanged();
                UInt32ToBytesCommand.OnCanExecuteChanged();
                Int64ToBytesCommand.OnCanExecuteChanged();
                UInt64ToBytesCommand.OnCanExecuteChanged();
                FloatToBytesCommand.OnCanExecuteChanged();
                DoubleToBytesCommand.OnCanExecuteChanged();
                FromBase64Command.OnCanExecuteChanged();
            }
        }
    }
}
