using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ConnectionManster.UI.PC.Converters
{
    public abstract class EnumTypeConverter<T> : IValueConverter
        where T:Enum
    {
        public Dictionary<T, string> Maps { get; }

        protected EnumTypeConverter()
        {
            Maps = new Dictionary<T, string>();
            ConfigMap();
        }

        protected abstract void ConfigMap();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }
            if(targetType == typeof(T))
            {
                var pair = (KeyValuePair<T, string>)value;
                var name = pair.Value;
                return Maps.Where(kv => kv.Value == name).Select(kv => kv.Key).First();
            }
            var key = (T)value;
            return Maps.First(kv =>kv.Key.Equals(key));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
