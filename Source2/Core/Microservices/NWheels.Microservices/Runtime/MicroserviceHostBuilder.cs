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

namespace NWheels.Microservices.Runtime
{
    public class MicroserviceHostBuilder
    {
        private readonly MutableBootConfiguration _bootConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder(string microserviceName)
        {
            _bootConfig = new MutableBootConfiguration() {
                MicroserviceName = microserviceName
            };
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
            var contribution = new ComponentContribution(contributor);
            _bootConfig.BootComponents.Register(builder => builder.RegisterComponentInstance(contribution));
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
                    var version = Assembly.GetEntryAssembly().GetName().Version;
                    ColorConsole.LogHeading($"Service '{_bootConfig.MicroserviceName}' version {version}");

                    _bootConfig.BootComponents.Register(RegisterCliHostComponents);

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

        private static void RegisterCliHostComponents(IComponentContainerBuilder builder)
        {
            builder.RegisterComponentType<ConsoleMicroserviceHostLogger>()
                .SingleInstance()
                .ForService<IMicroserviceHostLogger>()
                .AsFallback();

            builder.RegisterComponentType<DefaultModuleLoader>()
                .SingleInstance()
                .ForService<IModuleLoader>()
                .AsFallback();

            builder.RegisterComponentType<DefaultMicroserviceStateCodeBehind>()
                .InstancePerDependency()
                .ForService<IStateMachineCodeBehind<MicroserviceState, MicroserviceTrigger>>()
                .AsFallback();

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
