using ConnectionManster.UI.PC;
using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class LoggerViewModel : ObservableObject
    {
        public LoggerViewModel()
        {
            ClearCommand = new Command(Clear);
        }

        private StringBuilder _text = new StringBuilder();
        public string Text => _text.ToString();

        public Command ClearCommand { get; }

        private void Clear()
        {
            _text.Clear();
            OnTextChanged();
        }

        public void Append(string log)
        {
            _text.AppendLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]:{log}");
            OnTextChanged();
        }

        private void OnTextChanged()
        {
            OnPropertyChanged(nameof(Text));
        }
    }
}
