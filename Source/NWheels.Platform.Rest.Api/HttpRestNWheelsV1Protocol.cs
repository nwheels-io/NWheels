using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public static class HttpRestNWheelsV1Protocol
    {
        public static readonly string ProtocolName = "http/rest/nwheels/v1";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Info HttpRestNWheelsV1(this MessageProtocolInfo.IExtension extension)
        {
            return new Info();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Info
        {
            public string ProtocolName => HttpRestNWheelsV1Protocol.ProtocolName;
        }
    }
}
