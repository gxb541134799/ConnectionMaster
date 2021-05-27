using System;
using System.Runtime.Serialization;

namespace ConnectionMaster.Nat
{
    [Serializable]
    public class UnknowMessageException : Exception
    {
        public UnknowMessageException()
        {
        }

        protected UnknowMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}