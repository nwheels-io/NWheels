using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Kernel.Api.Extensions;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.Api.Exceptions
{
    [Serializable]
    public class MicroserviceHostException : ExplainableExceptionBase
    {
        private MicroserviceHostException(
            string reason, 
            Exception innerException = null,
            MicroserviceState? state = null,
            MicroserviceState? requiredState = null,
            Type failedClass = null,
            string failedPhase = null)
            : base(reason, innerException)
        {
            State = state;
            RequiredState = requiredState;
            FailedClass = failedClass;
            FailedPhase = failedPhase;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MicroserviceHostException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) 
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceState? State { get; }
        public MicroserviceState? RequiredState { get; }
        public Type FailedClass { get; }
        public string FailedPhase { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
        {
            if (State.HasValue)
            {
                yield return new KeyValuePair<string, string>(_s_stringState, State.ToString());
            }
            if (RequiredState.HasValue)
            {
                yield return new KeyValuePair<string, string>(_s_stringRequiredState, RequiredState.ToString());
            }
            if (FailedClass != null)
            {
                yield return new KeyValuePair<string, string>(_s_stringFailedClass, FailedClass.FriendlyName());
            }
            if (!string.IsNullOrEmpty(FailedPhase))
            {
                yield return new KeyValuePair<string, string>(_s_stringFailedPhase, FailedPhase);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_stringMicroserviceFaulted = nameof(MicroserviceFaulted);
        private static readonly string _s_stringMicroserviceDidNotReachRequiredState = nameof(MicroserviceDidNotReachRequiredState);
        private static readonly string _s_stringInvalidStateForStop = nameof(InvalidStateForStop);
        private static readonly string _s_stringNotConfiguredToRunInDaemonMode = nameof(NotConfiguredToRunInDaemonMode);
        private static readonly string _s_stringNotConfiguredToRunInBatchJobMode = nameof(NotConfiguredToRunInBatchJobMode);
        private static readonly string _s_stringFeatureLoaderFailed = nameof(FeatureLoaderFailed);
        private static readonly string _s_stringLifecycleComponentFailed = nameof(LifecycleComponentFailed);
        private static readonly string _s_stringState = nameof(State);
        private static readonly string _s_stringRequiredState = nameof(RequiredState);
        private static readonly string _s_stringFailedClass = nameof(FailedClass);
        private static readonly string _s_stringFailedPhase = nameof(FailedPhase);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostException MicroserviceFaulted(Exception innerException)
        {
            return new MicroserviceHostException(_s_stringMicroserviceFaulted, innerException);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostException MicroserviceDidNotReachRequiredState(MicroserviceState required, MicroserviceState actual)
        {
            return new MicroserviceHostException(
                reason: _s_stringMicroserviceDidNotReachRequiredState, 
                state: actual,
                requiredState: required);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostException InvalidStateForStop(MicroserviceState state)
        {
            return new MicroserviceHostException(_s_stringInvalidStateForStop, state: state);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostException NotConfiguredToRunInDaemonMode()
        {
            return new MicroserviceHostException(_s_stringNotConfiguredToRunInDaemonMode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostException NotConfiguredToRunInBatchJobMode()
        {
            return new MicroserviceHostException(_s_stringNotConfiguredToRunInBatchJobMode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostException FeatureLoaderFailed(Type loaderType, string phase, Exception error)
        {
            return new MicroserviceHostException(
                reason: _s_stringFeatureLoaderFailed,
                failedClass: loaderType,
                failedPhase: phase,
                innerException: error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MicroserviceHostException LifecycleComponentFailed(Type componentType, string phase, Exception error)
        {
            return new MicroserviceHostException(
                reason: _s_stringLifecycleComponentFailed,
                failedClass: componentType,
                failedPhase: phase,
                innerException: error);
        }
    }
}
