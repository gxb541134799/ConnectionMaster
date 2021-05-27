using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionMaster.Nat.Messages
{
    public enum NatMessageType
    {
        Join = 1,
        FindClient,
        FindClientResponse,
        ConnectClient
    }

    public abstract class NatMessage
    {
        public abstract NatMessageType MessageType { get; }
    }
}
