using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Kernel.Api.Exceptions
{
    // TODO: implement according to exception guidelines
    public class SafeLockException : ExplainableExceptionBase
    {
        private SafeLockException(string reason) : base(reason)
        {
        }

        protected override string[] BuildKeyValuePairs()
        {
            throw new NotImplementedException();
        }

        public static SafeLockException TimedOutWaitingForAccess(string resourceName, TimeSpan timeout)
        {
            return new SafeLockException(resourceName); //TODO: include parameters
        }
    }
}
