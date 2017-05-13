using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public class HttpRestNWheelsV1Protocol : HttpResourceProtocolBase
    {
        public static readonly string ProtocolNameString = "http/rest/nwheels/v1";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HttpRestNWheelsV1Protocol() 
            : base(ProtocolNameString)
        {
        }
    }
}
