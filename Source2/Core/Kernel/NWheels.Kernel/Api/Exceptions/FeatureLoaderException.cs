using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NWheels.Kernel.Api.Extensions;

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

        protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
        {
            yield return new KeyValuePair<string, string>("featureLoaderType", this.FeatureLoaderType.FriendlyFullName(fullNameGenericArgs: false));
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
