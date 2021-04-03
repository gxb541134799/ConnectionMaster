using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConnectionManster.UI.PC.Formatters
{
    public class BinaryMessageFormatter : IMessageFormatter
    {
        public string Name => "字节";

        public string FromBytes(byte[] bytes)
        {
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }

        public byte[] FromString(string text)
        {
            return text.Split(' ')
                .Select(value => byte.Parse(value, NumberStyles.HexNumber))
                .ToArray();
        }
    }
}
