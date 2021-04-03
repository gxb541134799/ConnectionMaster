using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionManster.UI.PC.Formatters
{
    public interface IMessageFormatter
    {
        string Name { get; }

        string FromBytes(byte[] bytes);

        byte[] FromString(string text);
    }
}
