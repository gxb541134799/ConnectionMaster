using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionMaster.Core
{
    public class TextMessage:MessageBase
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return Text?.ToString();
        }
    }
}
