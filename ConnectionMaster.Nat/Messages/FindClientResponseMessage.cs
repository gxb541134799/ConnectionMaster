using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ConnectionMaster.Nat.Messages
{
    public class FindClientResponseMessage : NatMessage
    {
        public IPEndPoint EndPoint { get; }

        public FindClientResponseMessage(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public override NatMessageType MessageType => NatMessageType.FindClientResponse;
    }
}
