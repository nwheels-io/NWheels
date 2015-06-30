using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    public abstract class DomainFaultException : Exception
    {
        public abstract object GetFaultValue();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainFaultException<TFault> : DomainFaultException
    {
        public DomainFaultException(TFault fault)
        {
            this.Fault = fault;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object GetFaultValue()
        {
            return this.Fault;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TFault Fault { get; private set; }
    }
}
