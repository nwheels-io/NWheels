using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints.Core;

namespace NWheels.Exceptions
{
    public class RemotePartyFaultException : Exception, IFaultException
    {
        public RemotePartyFaultException(string faultCode)
            : base("Remote party retured an error (code " + faultCode + ")")
        {
            this.FaultType = CompactRpcProtocol.FaultCodeRemoteParty;
            this.FaultCode = faultCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IFaultException

        public string FaultType { get; private set; }
        public string FaultCode { get; private set; }
        public string FaultSubCode { get; private set; }
        public string FaultReason { get; private set; }

        #endregion
    }
}
