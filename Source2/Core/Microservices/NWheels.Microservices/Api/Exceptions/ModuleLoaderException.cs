using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.Api.Exceptions
{
    [Serializable]
    public class ModuleLoaderException : ExplainableExceptionBase
    {
        private ModuleLoaderException(string reason, string moduleName = null, string featureName = null)
            : base(reason)
        {
            this.ModuleName = moduleName;
            this.FeatureName = featureName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ModuleLoaderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ModuleName { get; }
        public string FeatureName { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
        {
            yield return new KeyValuePair<string, string>(_s_stringModuleName, this.ModuleName);
            yield return new KeyValuePair<string, string>(_s_stringFeatureName, this.FeatureName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly static string _s_stringNamedFeatureDoesNotExist = nameof(NamedFeatureDoesNotExist);
        private readonly static string _s_stringDuplicateNamedFeature = nameof(DuplicateNamedFeature);
        private readonly static string _s_stringModuleName = nameof(ModuleName);
        private readonly static string _s_stringFeatureName = nameof(FeatureName);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ModuleLoaderException NamedFeatureDoesNotExist(string moduleName, string featureName)
        {
            return new ModuleLoaderException(
                reason: _s_stringNamedFeatureDoesNotExist,
                moduleName: moduleName,
                featureName: featureName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ModuleLoaderException DuplicateNamedFeature(string moduleName, string featureName)
        {
            return new ModuleLoaderException(
                reason: _s_stringDuplicateNamedFeature,
                moduleName: moduleName,
                featureName: featureName);
        }
    }
}
