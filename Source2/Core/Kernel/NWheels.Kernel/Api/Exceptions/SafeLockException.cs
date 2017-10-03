using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NWheels.Kernel.Api.Exceptions
{
    [Serializable]
    public class SafeLockException : ExplainableExceptionBase
    {
        private SafeLockException(string reason, string resourceName, TimeSpan timeout) 
            : base(reason)
        {
            this.ResourceName = resourceName;
            this.Timeout = timeout;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected SafeLockException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ResourceName { get; }
        public TimeSpan Timeout { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
        {
            yield return new KeyValuePair<string, string>(_s_stringResourceName, this.ResourceName);
            yield return new KeyValuePair<string, string>(_s_stringTimeout, this.Timeout.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_stringTimedOutWaitingForAccess = nameof(TimedOutWaitingForAccess);
        private static readonly string _s_stringResourceName = nameof(ResourceName);
        private static readonly string _s_stringTimeout = nameof(Timeout);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static SafeLockException TimedOutWaitingForAccess(string resourceName, TimeSpan timeout)
        {
            return new SafeLockException(_s_stringTimedOutWaitingForAccess, resourceName, timeout); 
        }
    }
}
