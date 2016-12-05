using System.Collections.Generic;
using Autofac;
using NWheels.Configuration.Core;

namespace NWheels.Configuration.Impls
{
    public class ProgrammaticConfigurationSource : ConfigurationSourceBase
    {
        private readonly Pipeline<ProgrammaticConfiguration> _configurations;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProgrammaticConfigurationSource(Pipeline<ProgrammaticConfiguration> configurations)
            : base("Programmatic", ConfigurationSourceLevel.Code)
        {
            _configurations = configurations;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConfigurationSourceBase

        public override void ApplyConfigurationProgrammatically(IComponentContext components)
        {
            for (int i = 0 ; i < _configurations.Count ; i++)
            {
                _configurations[i].Apply(components);
            }
        }

        #endregion
    }
}
