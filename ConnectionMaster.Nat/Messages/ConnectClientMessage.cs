using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionMaster.Nat.Messages
{
    public class ConnectClientMessage : NatMessage
    {
        public override NatMessageType MessageType => NatMessageType.ConnectClient;

        public string ClientId { get; }

        public ConnectClientMessage(string clientId)
        {
            ClientId = clientId;
        }
    }
}
