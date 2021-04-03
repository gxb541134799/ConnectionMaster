using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.Formatters
{
    public class Base64Formatter : IMessageFormatter
    {
        public string Name => "Base64";

        public string FromBytes(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public byte[] FromString(string text)
        {
            return Convert.FromBase64String(text);
        }
    }
}
