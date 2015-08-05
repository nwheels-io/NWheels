using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message)
            : base(message)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BusinessRuleException(string messageOrFormat, params object[] formatArgs)
            : base(messageOrFormat.FormatIf(formatArgs))
        {
        }
    }
}
