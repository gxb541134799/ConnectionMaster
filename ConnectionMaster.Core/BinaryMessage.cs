using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConnectionMaster.Core
{
    public class BinaryMessage:MessageBase
    {
        public byte[] Bytes { get; set; }

        public override string ToString()
        {
            if(Bytes == null)
            {
                return null;
            }
            return string.Join(" ", Bytes.Select(b => b.ToString("X2")));
        }
    }
}
