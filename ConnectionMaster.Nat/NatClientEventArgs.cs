using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ConnectionMaster.Nat
{
    public class NatClientEventArgs
    {
        public NatClientEventArgs(string clientId, IPEndPoint clientPoint)
        {
            ClientId = clientId;
            ClientPoint = clientPoint;
        }

        public string ClientId { get; }

        public IPEndPoint ClientPoint { get; }
    }
}
