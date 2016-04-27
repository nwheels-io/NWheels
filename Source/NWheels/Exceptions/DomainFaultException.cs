using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Exceptions
{
    public abstract class DomainFaultException : Exception, IFaultException
    {
        protected DomainFaultException(string message) : base(message)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IFaultException.FaultType
        {
            get
            {
                return this.GetFaultTypeString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IFaultException.FaultCode
        {
            get
            {
                return this.GetFaultCodeString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IFaultException.FaultSubCode
        {
            get
            {
                return this.GetFaultSubCodeString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string GetFaultTypeString();
        public abstract string GetFaultCodeString();
        public abstract string GetFaultSubCodeString();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FaultReason { get; protected set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainFaultException<TFaultCode> : DomainFaultException, IFaultException
    {
        public DomainFaultException(TFaultCode faultCode) 
            : base(faultCode.ToString())
        {
            this.FaultCode = faultCode;
            this.FaultReason = faultCode.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainFaultException(TFaultCode faultCode, string reasonFormat, params object[] reasonFormatArgs)
            : base(faultCode.ToString() + ": " + reasonFormat.FormatIf(reasonFormatArgs))
        {
            this.FaultCode = faultCode;
            this.FaultReason = reasonFormat.FormatIf(reasonFormatArgs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TFaultCode FaultCode { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string GetFaultTypeString()
        {
            return typeof(TFaultCode).Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public override string GetFaultCodeString()
        {
            return this.FaultCode.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string GetFaultSubCodeString()
        {
            return string.Empty;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainFaultException<TFaultCode, TFaultSubCode> : DomainFaultException, IFaultException
    {
        public DomainFaultException(TFaultCode faultCode, TFaultSubCode faultSubCode)
            : base(faultCode + "." + faultSubCode)
        {
            this.FaultCode = faultCode;
            this.FaultSubCode = faultSubCode;
            this.FaultReason = faultCode + "." + faultSubCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainFaultException(TFaultCode faultCode, TFaultSubCode faultSubCode, string reasonFormat, params object[] reasonFormatArgs)
            : base(faultCode + "." + faultSubCode + ": " + reasonFormat.FormatIf(reasonFormatArgs))
        {
            this.FaultCode = faultCode;
            this.FaultSubCode = faultSubCode;
            this.FaultReason = reasonFormat.FormatIf(reasonFormatArgs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TFaultCode FaultCode { get; private set; }
        public TFaultSubCode FaultSubCode { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string GetFaultTypeString()
        {
            return typeof(TFaultCode).Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string GetFaultCodeString()
        {
            return this.FaultCode.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string GetFaultSubCodeString()
        {
            return this.FaultSubCode.ToString();
        }
    }
}
