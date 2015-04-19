using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using NWheels.Configuration;
using NWheels.Conventions;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Endpoints;
using NWheels.Endpoints.Core.Wcf;
using NWheels.Entities;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.UI;
using NWheels.Processing;

namespace NWheels.Extensions
{
    public static class AutofacExtensions
    {
        public static TService ResolveAuto<TService>(this IComponentContext container)
            where TService : class
        {
            return container.Resolve<Auto<TService>>(TypedParameter.From(container)).Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool TryGetImplementationType(this IComponentContext container, Type contractType, out Type implementationType)
        {
            implementationType = container.ComponentRegistry.RegistrationsFor(new TypedService(contractType))
                .Select(x => x.Activator)
                .OfType<ReflectionActivator>()
                .Select(x => x.LimitType)
                .FirstOrDefault();

            return (implementationType != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsServiceRegisteredAsSingleton(this IComponentContext container, Type serviceType)
        {
            IComponentRegistration registration;

            if ( !container.ComponentRegistry.TryGetRegistration(new TypedService(serviceType), out registration) )
            {
                throw new InvalidOperationException("Specified service could not be found in the container.");
            }

            return (registration.Sharing == InstanceSharing.Shared && registration.Lifetime is RootScopeLifetime);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static AutofacExtensions.NWheelsFeatureRegistrations NWheelsFeatures(this ContainerBuilder builder)
        {
            return new NWheelsFeatureRegistrations(builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static LoggingFeature Logging(this NWheelsFeatureRegistrations features)
        {
            return new LoggingFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConfigurationFeature Configuration(this NWheelsFeatureRegistrations features)
        {
            return new ConfigurationFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ObjectContractFeature ObjectContracts(this NWheelsFeatureRegistrations features)
        {
            return new ObjectContractFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityFeature Entities(this NWheelsFeatureRegistrations features)
        {
            return new EntityFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ApiFeature Api(this NWheelsFeatureRegistrations features)
        {
            return new ApiFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static JobFeature Jobs(this NWheelsFeatureRegistrations features)
        {
            return new JobFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UIFeature UI(this NWheelsFeatureRegistrations features)
        {
            return new UIFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UseWcfForSoapEndpoints(this ContainerBuilder builder)
        {
            builder.RegisterType<WcfEndpointComponent>();
            builder.RegisterAdapter<SoapApiEndpointRegistration, WcfEndpointComponent>(
                (context, endpoint) => context.Resolve<WcfEndpointComponent>(TypedParameter.From(endpoint)))
                .As<ILifecycleEventListener>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UIAppEndpointRegistrations<TApp> WithWebEndpoint<TApp>(
            this UIAppEndpointRegistrations<TApp> registration, 
            string name = null, 
            string defaultUrl = null,
            bool exposeExceptions = false) 
            where TApp : class, IUiApplication
        {
            ((IHaveContainerBuilder)registration).Builder.RegisterInstance(new WebAppEndpointRegistration(name, typeof(TApp), defaultUrl, exposeExceptions));
            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ApiEndpointRegistration<TContract> WithSoapEndpoint<TContract>(
            this ApiEndpointRegistration<TContract> registration, 
            string name = null, 
            string defaultListenUrl = null,
            bool publishMetadata = true,
            string defaultMetadataUrl = null,
            bool exposeExceptions = false) 
            where TContract : class
        {
            ((IHaveContainerBuilder)registration).Builder.RegisterInstance(new SoapApiEndpointRegistration(
                name, typeof(TContract), defaultListenUrl, defaultMetadataUrl, publishMetadata, exposeExceptions));
            
            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static  DataRepositoryRegistration<TRepo> WithRestEndpoint<TRepo>(
            this DataRepositoryRegistration<TRepo> registration, 
            string name = null, 
            string defaultListenUrl = null,
            bool publishMetadata = true,
            string defaultMetadataUrl = null,
            bool exposeExceptions = false) 
            where TRepo : class, IApplicationDataRepository
        {
            ((IHaveContainerBuilder)registration).Builder.RegisterInstance(new RestApiEndpointRegistration(
                name, typeof(TRepo), defaultListenUrl, defaultMetadataUrl, publishMetadata, exposeExceptions));

            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ApiEndpointRegistration<TContract> WithJsonEndpoint<TContract>(
            this ApiEndpointRegistration<TContract> registration, 
            string name = null, 
            string defaultListenUrl = null,
            bool publishMetadata = true,
            string defaultMetadataUrl = null,
            bool exposeExceptions = false)
            where TContract : class
        {
            ((IHaveContainerBuilder)registration).Builder.RegisterInstance(new JsonApiEndpointRegistration(
                name, typeof(TContract), defaultListenUrl, defaultMetadataUrl, publishMetadata, exposeExceptions));

            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IHaveContainerBuilder
        {
            ContainerBuilder Builder { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NWheelsFeatureRegistrations : IHaveContainerBuilder
        {
            private readonly ContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NWheelsFeatureRegistrations(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            ContainerBuilder IHaveContainerBuilder.Builder
            {
                get { return _builder; }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class LoggingFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public LoggingFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterLogger<TLogger>()
                where TLogger : class, IApplicationEventLogger
            {
                _builder.Register(ctx => ctx.Resolve<LoggerObjectFactory>().CreateService<TLogger>()).As<TLogger>();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConfigurationFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ConfigurationFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterSection<TSection>()
                where TSection : class, IConfigurationSection
            {
                _builder.RegisterType<ConfigSectionRegistration<TSection>>().As<IConfigSectionRegistration>().InstancePerDependency();
                _builder.Register(ctx => ctx.Resolve<ConfigurationObjectFactory>().CreateService<TSection>()).As<TSection>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ObjectContractFeature
        {
            private readonly ContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ObjectContractFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractConcretizationRegistration<TGeneral> Concretize<TGeneral>() where TGeneral : class
            {
                return new ContractConcretizationRegistration<TGeneral>(_builder);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractMixinRegistration<TPart> Mix<TPart>() where TPart : class
            {
                return new ContractMixinRegistration<TPart>(_builder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityFeature
        {
            private readonly ContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DataRepositoryRegistration<TRepo> RegisterDataRepository<TRepo>() where TRepo : class, IApplicationDataRepository
            {
                var registration = new DataRepositoryRegistration<TRepo>(_builder);
                _builder.RegisterInstance(registration).As<DataRepositoryRegistration>();
                return registration;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void UsePascalCaseRelationalMappingConvention(bool usePluralTableNames = true)
            {
                _builder.RegisterInstance(new RelationalMappingConventionDefault(
                    RelationalMappingConventionDefault.ConventionType.PascalCase, 
                    usePluralTableNames));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void UseUnderscoreRelationalMappingConvention(bool usePluralTableNames = true)
            {
                _builder.RegisterInstance(new RelationalMappingConventionDefault(
                    RelationalMappingConventionDefault.ConventionType.Underscore, 
                    usePluralTableNames));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterCustomRelationalMappingConvention<TConvention>(bool singleInstance = true)
                where TConvention : IRelationalMappingConvention
            {
                var registration = _builder.RegisterType<TConvention>().As<IRelationalMappingConvention>();

                if ( singleInstance )
                {
                    registration.SingleInstance();
                }
                else
                {
                    registration.InstancePerDependency();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterRelationalMappingFineTune<TEntity>(Action<IRelationalMappingFineTune<TEntity>> fineTuneAction)
                where TEntity : class
            {
                var fineTuner = new RelationalMappingFineTuner<TEntity>(fineTuneAction);
                _builder.RegisterInstance<RelationalMappingFineTuner<TEntity>>(fineTuner);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ApiFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ApiFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ApiEndpointRegistration<TContract> RegisterContract<TContract>() 
                where TContract : class
            {
                return new ApiEndpointRegistration<TContract>(_builder);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class JobFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public JobFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterJob<TJob>()
                where TJob : IApplicationJob
            {
                _builder.RegisterType<TJob>().As<TJob, IApplicationJob>().InstancePerDependency();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class UIFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIAppEndpointRegistrations<TApp> RegisterApplication<TApp>() where TApp : class, IUiApplication
            {
                _builder.RegisterType<TApp>().As<TApp, IUiApplication>();
                return new UIAppEndpointRegistrations<TApp>(_builder);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ContractConcretizationRegistration<TGeneral> where TGeneral : class
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractConcretizationRegistration(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void With<TConcrete>() where TConcrete : class, TGeneral
            {
                var concretization = new ConcretizationRegistration(typeof(TGeneral), typeof(TConcrete));
                _builder.RegisterInstance(concretization).As<ConcretizationRegistration>();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ContractMixinRegistration<TPart> where TPart : class
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractMixinRegistration(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Into<TContract>() where TContract : class
            {
                var mixin = new MixinRegistration(typeof(TContract), typeof(TPart));
                _builder.RegisterInstance(mixin).As<MixinRegistration>();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ApiEndpointRegistration<TContract> : IHaveContainerBuilder
            where TContract : class
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ApiEndpointRegistration(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            ContainerBuilder IHaveContainerBuilder.Builder
            {
                get { return _builder; }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class UIAppEndpointRegistrations<TApp> : IHaveContainerBuilder
            where TApp : class, IUiApplication
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIAppEndpointRegistrations(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            ContainerBuilder IHaveContainerBuilder.Builder
            {
                get { return _builder; }
            }
        }
    }
}
