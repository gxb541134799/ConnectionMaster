using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class SymmetricViewModel:ObservableObject
    {
        public SymmetricViewModel(FormatterViewModel formatterViewModel)
        {
            FormatterViewModel = formatterViewModel;
            AlgorithmNames = new string[]
            {
                "AES",
                "RC2",
                "Rijndael",
                "DES",
                "TripleDES"
            };
            AlgorithmName = AlgorithmNames[0];
            EncryptCommand = new Command(Encrypt,CanTransform);
            DecryptCommand = new Command(Decrypt,CanTransform);
        }

        public FormatterViewModel FormatterViewModel { get; }

        public string[] AlgorithmNames { get; }

        public string AlgorithmName { get; set; }

        private string _key;
        public string Key
        {
            get { return _key; }
            set { SetValue(ref _key, value, nameof(Key)); }
        }

        private string _iv;
        public string IV
        {
            get { return _iv; }
            set { SetValue(ref _iv, value, nameof(IV)); }
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
            private set { SetValue(ref _output, value, nameof(Output)); }
        }

        public Command EncryptCommand { get; }

        public Command DecryptCommand { get; }

        private void Encrypt()
        {
            Transform((aes, key, iv) => aes.CreateEncryptor(key, iv));
        }

        private void Decrypt()
        {
            Transform((aes, key, iv) => aes.CreateDecryptor(key, iv));
        }

        private bool CanTransform()
        {
            return !string.IsNullOrEmpty(Input)
                && !string.IsNullOrEmpty(Key)
                && !string.IsNullOrEmpty(IV);
        }

        private void Transform(Func<SymmetricAlgorithm, byte[],byte[],ICryptoTransform> transformFactory)
        {
            var formatter = FormatterViewModel.Formatter;
            try
            {
                var keyBytes = formatter.FromString(Key);
                var ivBytes = formatter.FromString(IV);
                var inputBytes = formatter.FromString(Input);
                using (var aes = SymmetricAlgorithm.Create(AlgorithmName))
                {
                    var transform = transformFactory(aes, keyBytes, ivBytes);
                    var outputBytes = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    Output = formatter.FromBytes(outputBytes);
                }
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "失败");
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            var propertyNames = new HashSet<string>()
            {
                nameof(Input),
                nameof(Key),
                nameof(IV)
            };
            if(propertyNames.Contains(propertyName))
            {
                EncryptCommand.OnCanExecuteChanged();
                DecryptCommand.OnCanExecuteChanged();
            }
        }
    }
}
