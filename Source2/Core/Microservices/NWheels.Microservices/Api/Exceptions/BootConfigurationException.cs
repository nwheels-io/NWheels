using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.Api.Exceptions
{
    [Serializable]
    public class BootConfigurationException : ExplainableExceptionBase
    {
        private BootConfigurationException(string reason, string moduleName = null, string featureName = null)
            : base(reason)
        {
            this.ModuleName = moduleName;
            this.FeatureName = featureName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected BootConfigurationException(SerializationInfo info, StreamingContext context)
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

        private readonly static string _s_stringKernelModuleWrongOrder = nameof(KernelModuleWrongOrder);
        private readonly static string _s_stringModuleName = nameof(ModuleName);
        private readonly static string _s_stringFeatureName = nameof(FeatureName);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BootConfigurationException KernelModuleWrongOrder()
        {
            return new BootConfigurationException(
                reason: _s_stringKernelModuleWrongOrder, 
                moduleName: MutableBootConfiguration.KernelAssemblyName);
        }
    }
}
