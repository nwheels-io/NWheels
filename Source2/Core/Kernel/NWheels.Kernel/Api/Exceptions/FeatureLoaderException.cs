using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NWheels.Kernel.Api.Exceptions
{
    [Serializable]
    public class FeatureLoaderException : ExplainableExceptionBase
    {
        private FeatureLoaderException(string reason, Type featureLoaderType) 
            : base(reason)
        {
            this.FeatureLoaderType = featureLoaderType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected FeatureLoaderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type FeatureLoaderType { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string[] BuildKeyValuePairs()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_stringFeatureNameMissingOrInvalid = nameof(FeatureNameMissingOrInvalid);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FeatureLoaderException FeatureNameMissingOrInvalid(Type featureLoaderType)
        {
            return new FeatureLoaderException(_s_stringFeatureNameMissingOrInvalid, featureLoaderType);
        }
    }
}
