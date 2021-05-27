using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ConnectionMaster.Nat.Messages
{
    public class FindClientMessage:NatMessage
    {
        public string ClientId { get; }

        public FindClientMessage(string clientId)
        {
            ClientId = clientId;
        }

        public override NatMessageType MessageType => NatMessageType.FindClient;
    }
}
