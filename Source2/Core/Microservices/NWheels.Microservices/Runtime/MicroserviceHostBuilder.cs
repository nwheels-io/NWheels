using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using NWheels.Kernel.Api.Injection;
using System.Xml.Linq;
using NWheels.Kernel.Api.Primitives;

namespace NWheels.Microservices.Runtime
{
    public class MicroserviceHostBuilder
    {
        private readonly MutableBootConfiguration _bootConfig;
        private readonly List<Action<IComponentContainer, IComponentContainerBuilder>> _componentContributions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder(string microserviceName)
        {
            _bootConfig = new MutableBootConfiguration() {
                MicroserviceName = microserviceName
            };

            _componentContributions = new List<Action<IComponentContainer, IComponentContainerBuilder>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseFrameworkFeature<TFeature>()
            where TFeature : class, IFeatureLoader, new()
        {
            _bootConfig.AddFeatures(_bootConfig.FrameworkModules, typeof(TFeature).Assembly, typeof(TFeature));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseApplicationFeature<TFeature>()
            where TFeature : class, IFeatureLoader, new()
        {
            _bootConfig.AddFeatures(_bootConfig.ApplicationModules, typeof(TFeature).Assembly, typeof(TFeature));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseCustomizationFeature<TFeature>()
            where TFeature : class, IFeatureLoader, new()
        {
            _bootConfig.AddFeatures(_bootConfig.CustomizationModules, typeof(TFeature).Assembly, typeof(TFeature));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder ContributeComponents(Action<IComponentContainer, IComponentContainerBuilder> contributor)
        {
            _componentContributions.Add(contributor);
            UseApplicationFeature<ContributionsFeatureLoader>();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseMicroserviceXml(XElement xml)
        {
            MicroserviceXmlReader.PopulateBootConfiguration(xml, _bootConfig);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHost Build()
        {
            return new MicroserviceHost(this.BootConfig);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IBootConfiguration BootConfig => _bootConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void ApplyComponentContributions(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            foreach (var contributor in _componentContributions)
            {
                contributor(existingComponents, newComponents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "MicroserviceHostBuilderContributions")]
        public class ContributionsFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                base.ContributeComponents(existingComponents, newComponents);

                var hostBuilder = existingComponents.Resolve<MicroserviceHostBuilder>();
                hostBuilder.ApplyComponentContributions(existingComponents, newComponents);
            }
        }
    }
}
