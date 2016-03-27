using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Extensions;

namespace NWheels.Exceptions
{
    public class ConventionException : Exception
    {
        public ConventionException(string message, params object[] formatArgs)
            : base(message.FormatIf(formatArgs))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public ConventionException(Exception innerException, string message, params object[] formatArgs)
            : base(message.FormatIf(formatArgs), innerException)
        {
        }
    }
}
