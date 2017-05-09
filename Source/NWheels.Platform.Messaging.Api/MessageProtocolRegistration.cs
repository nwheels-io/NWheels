using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    public class MessageProtocolRegistration
    {
        public MessageProtocolRegistration(Type protocolInterface, string protocolName)
        {
            this.ProtocolInterface = protocolInterface;
            this.ProtocolName = protocolName ?? string.Empty;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ProtocolInterface { get; }
        public string ProtocolName { get; }
    }
}
