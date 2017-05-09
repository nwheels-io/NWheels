using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    public interface IMessageProtocolInterface
    {
        Type ProtocolInterface { get; }
        string ProtocolName { get; }
    }
}
