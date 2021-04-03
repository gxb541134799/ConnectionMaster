using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConnectionManster.UI.PC.Formatters
{
    public class TextMessageFormatter : IMessageFormatter
    {
        public static Encoding[] Encodings { get; }

        static TextMessageFormatter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encodings = new Encoding[]
            {
                Encoding.UTF8,
                Encoding.Unicode,
                Encoding.ASCII,
                Encoding.GetEncoding("GB2312")
            };
        }

        public TextMessageFormatter()
        {
            Encoding = Encodings.FirstOrDefault();
        }

        public Encoding Encoding { get; set; }

        public string Name => "文本";

        public string FromBytes(byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }

        public byte[] FromString(string text)
        {
            return Encoding.GetBytes(text);
        }
    }
}
