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
            Type featureLoaderType = null,
            string featureLoaderPhase = null)
            : base(reason, innerException)
        {
            State = state;
            RequiredState = requiredState;
            FeatureLoaderType = featureLoaderType;
            FeatureLoaderPhase = featureLoaderPhase;
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
        public Type FeatureLoaderType { get; }
        public string FeatureLoaderPhase { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
        {
            yield return new KeyValuePair<string, string>(_s_stringState, State.ToString());
            yield return new KeyValuePair<string, string>(_s_stringRequiredState, RequiredState.ToString());
            yield return new KeyValuePair<string, string>(_s_stringFeatureLoaderType, FeatureLoaderType?.FriendlyName());
            yield return new KeyValuePair<string, string>(_s_stringFeatureLoaderPhase, FeatureLoaderPhase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_stringMicroserviceFaulted = nameof(MicroserviceFaulted);
        private static readonly string _s_stringMicroserviceDidNotReachRequiredState = nameof(MicroserviceDidNotReachRequiredState);
        private static readonly string _s_stringInvalidStateForStop = nameof(InvalidStateForStop);
        private static readonly string _s_stringNotConfiguredToRunInDaemonMode = nameof(NotConfiguredToRunInDaemonMode);
        private static readonly string _s_stringNotConfiguredToRunInBatchJobMode = nameof(NotConfiguredToRunInBatchJobMode);
        private static readonly string _s_stringFeatureLoaderFailed = nameof(FeatureLoaderFailed);
        private static readonly string _s_stringState = nameof(State);
        private static readonly string _s_stringRequiredState = nameof(RequiredState);
        private static readonly string _s_stringFeatureLoaderType = nameof(FeatureLoaderType);
        private static readonly string _s_stringFeatureLoaderPhase = nameof(FeatureLoaderPhase);

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
                featureLoaderPhase: phase,
                innerException: error);
        }
    }
}
