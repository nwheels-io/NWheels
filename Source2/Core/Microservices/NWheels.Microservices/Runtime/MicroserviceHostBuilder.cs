using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using NWheels.Kernel.Api.Injection;
using System.Xml.Linq;
using NWheels.Kernel.Api.Primitives;
using NWheels.Microservices.Runtime.Cli;

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

        /// <summary>
        /// Runs command-line interface for the microservice
        /// </summary>
        /// <param name="args">
        /// Command line arguments
        /// </param>
        /// <returns>
        /// Exit code to return to the OS: 
        ///    0 = success, 
        ///   -1 = failure during initialization, 
        ///   -2 = failure during execution
        ///   -3 = daemon didn't stop within allotted timeout
        /// </returns>
        /// <remarks>
        /// If command line arguments are invalid, or help is requested with -h, -?, or --help option, 
        /// this method will print appropriate output, then terminate the process with exit code 1.
        /// </remarks>
        public int RunCli(string[] args)
        {
            using (var cli = new CliProgram())
            {
                try
                {
                    _bootConfig.HostComponents.Register(builder => {
                        builder.RegisterComponentType<ConsoleMicroserviceHostLogger>().SingleInstance().ForService<IMicroserviceHostLogger>();
                        builder.RegisterComponentType<InspectCommand>().ForService<ICliCommand>();
                        builder.RegisterComponentType<CompileCommand>().ForService<ICliCommand>();
                        builder.RegisterComponentType<JobCommand>().ForService<ICliCommand>();
                        builder.RegisterComponentType<RunCommand>().ForService<ICliCommand>();
                    });

                    var host = Build();
                    return cli.Run(host, args);
                }
                catch (Exception e)
                {
                    cli.LogCrash(e, isTerminating: true);
                    return -1;
                }
            }
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
