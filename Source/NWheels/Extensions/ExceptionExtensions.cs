using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;

namespace NWheels.Extensions
{
    public static class ExceptionExtensions
    {
        public static readonly string FaultCodeUnknown = "Unknown";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetMessageDeep(this Exception exception)
        {
            var text = new StringBuilder();
            text.Append(exception.Message);

            for ( Exception inner = exception.InnerException ; inner != null ; inner = inner.InnerException )
            {
                text.Append(" -> ");
                text.Append(inner.Message);
            }

            var aggregate = exception as AggregateException;

            if ( aggregate != null )
            {
                foreach ( var inner in aggregate.InnerExceptions )
                {
                    text.Append(" -> ");
                    text.Append(GetMessageDeep(inner));
                }
            }

            return text.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Exception GetUserFriendlyException(this Exception e)
        {
            var aggregate = e as AggregateException;

            if (aggregate != null && aggregate.InnerExceptions.Count > 0)
            {
                return GetUserFriendlyException(aggregate.InnerExceptions.First());
            }

            var targetInvocation = e as TargetInvocationException;

            if (targetInvocation != null && targetInvocation.InnerException != null)
            {
                return GetUserFriendlyException(targetInvocation.InnerException);
            }

            return e;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetQualifiedFaultCode(this IFaultException fault)
        {
            var qualified = new StringBuilder(capacity: 64);
            
            var type = fault.FaultType;
            var code = fault.FaultCode;
            var subCode = fault.FaultSubCode;

            var hasType = string.IsNullOrEmpty(type);
            var hasCode = string.IsNullOrEmpty(code);
            var hasSubCode = string.IsNullOrEmpty(subCode);

            if (hasType)
            {
                qualified.Append(fault.FaultType);
            }

            if (hasCode)
            {
                if (qualified.Length > 0)
                {
                    qualified.Append('/');
                }

                qualified.Append(code);
            }

            if (hasSubCode)
            {
                if (qualified.Length > 0)
                {
                    qualified.Append('/');
                }

                qualified.Append(subCode);
            }

            if (qualified.Length > 0)
            {
                return qualified.ToString();
            }

            return FaultCodeUnknown;
        }
    }
}
