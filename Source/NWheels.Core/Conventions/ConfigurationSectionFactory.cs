using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Configuration;
using NWheels.Conventions;
using NWheels.Core.Configuration;

namespace NWheels.Core.Conventions
{
    internal class ConfigurationSectionFactory : ConventionObjectFactory, IAutoObjectFactory
    {
        private readonly Auto<IConfigurationLogger> _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConfigurationSectionFactory(DynamicModule module, Auto<IConfigurationLogger> logger)
            : base(module, new ConfigurationSectionConvention())
        {
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService CreateService<TService>() where TService : class
        {
            return base.CreateInstanceOf<TService>().UsingConstructor(_logger, string.Empty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ServiceAncestorMarkerType
        {
            get
            {
                return typeof(IConfigurationSection);
            }
        }
    }
}
