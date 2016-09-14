using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    public class InternalServerErrorFaultException : Exception, IFaultException
    {
        public static readonly string InternalErrorFaultType = "InternalServerError";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InternalServerErrorFaultException(Exception inner) 
            : base("Internal server error", inner)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IFaultException

        public string FaultType
        {
            get { return InternalErrorFaultType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FaultCode
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FaultSubCode 
        { 
            get { return null; } 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FaultReason
        {
            get { return null; }
        }

        #endregion
    }
}
