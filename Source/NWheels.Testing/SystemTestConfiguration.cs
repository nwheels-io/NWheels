using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;

namespace NWheels.Testing
{
    public static class SystemTestConfiguration
    {
        [AttributeUsage(AttributeTargets.Method)]
        public abstract class AttributeBase : Attribute
        {
            public virtual void OnBuildingBootConfig(BootConfiguration configuration)
            {
            }
            public virtual void OnRegisteringHostComponents(Autofac.ContainerBuilder builder)
            {
            }
            public virtual void OnRegisteringModuleComponents(Autofac.ContainerBuilder builder)
            {
            }
            public virtual void OnInitializingAgentComponent()
            {
            }
            protected virtual void ApplyTo(TestFixtureWithNodeHosts fixture)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class IncludeApplicationModulesAttribute : AttributeBase
        {
            public IncludeApplicationModulesAttribute(params string[] moduleNames)
            {
                ModuleNames = moduleNames;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void OnBuildingBootConfig(BootConfiguration configuration)
            {
                configuration.ApplicationModules.AddRange(ModuleNames.Select(name => new BootConfiguration.ModuleConfig() {
                    Assembly = name
                }));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] ModuleNames { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ShouldAutoStartApplicationAttribute : AttributeBase
        {
            public ShouldAutoStartApplicationAttribute(bool value)
            {
                Value = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Value { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ApplyTo(TestFixtureWithNodeHosts fixture)
            {
                base.ApplyTo(fixture);
                fixture.ShouldAutoStartApplication = Value;
            }
        }
    }
}
