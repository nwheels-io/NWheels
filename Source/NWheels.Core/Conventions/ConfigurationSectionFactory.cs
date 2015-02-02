using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Configuration;
using NWheels.Conventions;
using NWheels.Core.Configuration;
using NWheels.Extensions;

namespace NWheels.Core.Conventions
{
    internal class ConfigurationSectionFactory : ConventionObjectFactory, IAutoObjectFactory
    {
        private readonly IComponentContext _components;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConfigurationSectionFactory(IComponentContext components, DynamicModule module)
            : base(module, new ConfigurationSectionConvention())
        {
            _components = components;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService CreateService<TService>() where TService : class
        {
            return base.CreateInstanceOf<TService>().UsingConstructor(_components.Resolve<Auto<IConfigurationLogger>>() , string.Empty);
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
