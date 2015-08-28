using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Exceptions
{
    public class DomainFaultException<TFaultCode> : Exception, IFaultException
    {
        public DomainFaultException(TFaultCode faultCode)
        {
            this.FaultCode = faultCode;
            this.FaultReason = faultCode.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainFaultException(TFaultCode faultCode, string reasonFormat, params object[] reasonFormatArgs)
        {
            this.FaultCode = faultCode;
            this.FaultReason = reasonFormat.FormatIf(reasonFormatArgs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TFaultCode FaultCode { get; private set; }
        public string FaultReason { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IFaultException.FaultCode
        {
            get
            {
                return this.FaultCode.ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IFaultException.FaultSubCode
        {
            get
            {
                return string.Empty;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainFaultException<TFaultCode, TFaultSubCode> : Exception, IFaultException
    {
        public DomainFaultException(TFaultCode faultCode, TFaultSubCode faultSubCode)
        {
            this.FaultCode = faultCode;
            this.FaultSubCode = faultSubCode;
            this.FaultReason = faultCode + "." + faultSubCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainFaultException(TFaultCode faultCode, TFaultSubCode faultSubCode, string reasonFormat, params object[] reasonFormatArgs)
        {
            this.FaultCode = faultCode;
            this.FaultSubCode = faultSubCode;
            this.FaultReason = reasonFormat.FormatIf(reasonFormatArgs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TFaultCode FaultCode { get; private set; }
        public TFaultSubCode FaultSubCode { get; private set; }
        public string FaultReason { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IFaultException.FaultCode
        {
            get
            {
                return this.FaultCode.ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IFaultException.FaultSubCode
        {
            get
            {
                return this.FaultSubCode.ToString();
            }
        }
    }

}
