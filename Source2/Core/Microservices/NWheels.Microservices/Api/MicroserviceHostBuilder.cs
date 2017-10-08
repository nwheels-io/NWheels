using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using NWheels.Kernel.Api.Injection;
using System.Xml.Linq;
using NWheels.Kernel.Api.Primitives;
using NWheels.Microservices.Runtime.Cli;
using NWheels.Kernel.Api.Logging;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.Api
{
    public class MicroserviceHostBuilder
    {
        public MicroserviceHostBuilder(string microserviceName)
        {
            BootConfig = new MutableBootConfiguration() {
                MicroserviceName = microserviceName
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseFrameworkFeature<TFeature>()
            where TFeature : class, IFeatureLoader, new()
        {
            if (FeatureLoaderAttribute.IsNamedFeature(typeof(TFeature)))
            {
                BootConfig.AddFeatures(BootConfig.FrameworkModules, typeof(TFeature).Assembly, typeof(TFeature));
            }
            else
            {
                BootConfig.AddFeatures(BootConfig.FrameworkModules, typeof(TFeature).Assembly);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseApplicationFeature<TFeature>()
            where TFeature : class, IFeatureLoader, new()
        {
            if (FeatureLoaderAttribute.IsNamedFeature(typeof(TFeature)))
            {
                BootConfig.AddFeatures(BootConfig.ApplicationModules, typeof(TFeature).Assembly, typeof(TFeature));
            }
            else
            {
                BootConfig.AddFeatures(BootConfig.ApplicationModules, typeof(TFeature).Assembly);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseCustomizationFeature<TFeature>()
            where TFeature : class, IFeatureLoader, new()
        {
            if (FeatureLoaderAttribute.IsNamedFeature(typeof(TFeature)))
            {
                BootConfig.AddFeatures(BootConfig.CustomizationModules, typeof(TFeature).Assembly, typeof(TFeature));
            }
            else
            {
                BootConfig.AddFeatures(BootConfig.CustomizationModules, typeof(TFeature).Assembly);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseBootComponents(Action<IComponentContainerBuilder> registration)
        {
            BootConfig.BootComponents.Register(registration);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder ContributeComponents(Action<IComponentContainer, IComponentContainerBuilder> contributor)
        {
            var contribution = new ComponentContribution(contributor);
            BootConfig.BootComponents.Register(builder => builder.RegisterComponentInstance(contribution));
            UseApplicationFeature<ContributionsFeatureLoader>();

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseMicroserviceXml(XElement xml)
        {
            MicroserviceXmlReader.PopulateBootConfiguration(xml, BootConfig);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMicroserviceHost BuildHost()
        {
            return new MicroserviceHost(this.BootConfig);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMicroserviceHostCli BuildCli()
        {
            var cli = new MicroserviceHostCli();

            var version = Assembly.GetEntryAssembly().GetName().Version;
            ColorConsole.LogHeading($"Service '{BootConfig.MicroserviceName}' version {version}");

            BootConfig.BootComponents.Register(RegisterCliHostComponents);

            var host = new MicroserviceHost(this.BootConfig);
            cli.UseHost(host);

            return cli;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MutableBootConfiguration BootConfig { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void RegisterCliHostComponents(IComponentContainerBuilder builder)
        {
            builder.RegisterComponentType<InspectCommand>().ForService<ICliCommand>();
            builder.RegisterComponentType<CompileCommand>().ForService<ICliCommand>();
            builder.RegisterComponentType<JobCommand>().ForService<ICliCommand>();
            builder.RegisterComponentType<RunCommand>().ForService<ICliCommand>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "MicroserviceHostBuilderContributions")]
        public class ContributionsFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                base.ContributeComponents(existingComponents, newComponents);

                var host = existingComponents.Resolve<MicroserviceHost>();
                var contributions = host.GetBootComponents().ResolveAll<ComponentContribution>();

                foreach (var contribution in contributions)
                {
                    contribution.Apply(existingComponents, newComponents);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
    
        public class ComponentContribution
        {
            public ComponentContribution(Action<IComponentContainer, IComponentContainerBuilder> apply)
            {
                this.Apply = apply;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Action<IComponentContainer, IComponentContainerBuilder> Apply { get; }
        }
    }
}
