using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    public abstract class MessageProtocolBase : IMessageProtocolInterface
    {
        protected MessageProtocolBase(Type protocolInterface, string protocolName)
        {
            this.ProtocolInterface = protocolInterface;
            this.ProtocolName = protocolName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ProtocolInterface { get; }
        public string ProtocolName { get; }
    }
}
