using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;

namespace NWheels.Configuration
{
    public interface IConfigSectionRegistration
    {
        IConfigurationSection ResolveFromContainer(IComponentContext container);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class ConfigSectionRegistration<TSection> : IConfigSectionRegistration
        where TSection : class, IConfigurationSection
    {
        public IConfigurationSection ResolveFromContainer(IComponentContext container)
        {
            return container.ResolveAuto<TSection>();
        }
    }
}
