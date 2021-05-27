using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionMaster.Nat.Messages
{
    public class JoinMessage:NatMessage
    {
        public JoinMessage(string clientId)
        {
            ClientId = clientId;
        }

        public string ClientId { get; }

        public override NatMessageType MessageType => NatMessageType.Join;
    }
}
