using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Communication.Api
{
    public enum DisconnectReason
    {
        Unknown,
        ByContract,
        LocalPartyShutDown,
        RemotePartyNotReachable,
        ProtocolError,
        ServiceLevelAgreementNotMet        
    }
}
