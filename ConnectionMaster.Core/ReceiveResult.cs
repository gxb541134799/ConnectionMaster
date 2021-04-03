using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionMaster.Core
{
    public class ReceiveResult
    {
        public byte[] Data { get; }

        public object Payload { get; }

        public ReceiveResult(byte[] data,object payload = null)
        {
            Data = data;
            Payload = payload;
        }
    }
}
