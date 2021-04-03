using ConnectionManster.UI.PC.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class FormatterViewModel:ObservableObject
    {
        public FormatterViewModel(IEnumerable<IMessageFormatter> formatters)
        {
            Formatters = formatters.ToArray();
            Formatter = Formatters.FirstOrDefault();
        }

        public IEnumerable<IMessageFormatter> Formatters { get; }

        private IMessageFormatter _formatter;
        public IMessageFormatter Formatter
        {
            get { return _formatter; }
            set { SetValue(ref _formatter, value, nameof(Formatter)); }
        }
    }
}
