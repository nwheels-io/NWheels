using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.Api.Exceptions
{
    //TODO: implement according to guidelines
    public class MicroserviceHostException : ExplainableExceptionBase
    {
        private MicroserviceHostException(string reason)
            : base(reason)
        {
        }

        private MicroserviceHostException(string reason, Exception innerException)
            : base(reason, innerException)
        {
        }

        protected override string[] BuildKeyValuePairs()
        {
            throw new NotImplementedException();
        }

        public static MicroserviceHostException MicroserviceFaulted(Exception innerException)
        {
            return new MicroserviceHostException(nameof(MicroserviceFaulted), innerException);
        }

        public static MicroserviceHostException MicroserviceDidNotReachRequiredState(MicroserviceState required, MicroserviceState actual)
        {
            return new MicroserviceHostException(nameof(MicroserviceDidNotReachRequiredState)); //TODO: include parameter values
        }

        public static MicroserviceHostException InvalidStateForStop(MicroserviceState state)
        {
            return new MicroserviceHostException(nameof(InvalidStateForStop)); //TODO: include parameter values
        }

        public static MicroserviceHostException NotConfiguredToRunInDaemonMode()
        {
            return new MicroserviceHostException(nameof(NotConfiguredToRunInDaemonMode));
        }

        public static MicroserviceHostException NotConfiguredToRunInBatchJobMode()
        {
            return new MicroserviceHostException(nameof(NotConfiguredToRunInBatchJobMode));
        }
    }
}
