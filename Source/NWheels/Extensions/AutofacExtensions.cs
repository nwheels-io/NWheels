using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Hapil;
using NWheels.Authorization;
using NWheels.Configuration;
using NWheels.Conventions;
using NWheels.Conventions.Core;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Endpoints;
using NWheels.Endpoints.Core.Wcf;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Impl;
using NWheels.Entities.Migrations;
using NWheels.Exceptions;
using NWheels.Hosting;
using NWheels.Hosting.Factories;
using NWheels.Logging;
using NWheels.UI;
using NWheels.Processing;
using NWheels.Processing.Jobs;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Processing.Workflows.Impl;
using NWheels.TypeModel;
using NWheels.UI.Uidl;

namespace NWheels.Extensions
{
    public static partial class AutofacExtensions
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

        public static HostingFeature Hosting(this NWheelsFeatureRegistrations features)
        {
            return new HostingFeature(((IHaveContainerBuilder)features).Builder);
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

        public static CommunicationFeature Communication(this NWheelsFeatureRegistrations features)
        {
            return new CommunicationFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ObjectContractFeature ObjectContracts(this NWheelsFeatureRegistrations features)
        {
            return new ObjectContractFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static AuthorizationFeature Authorizarion(this NWheelsFeatureRegistrations features)
        {
            return new AuthorizationFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityFeature Entities(this NWheelsFeatureRegistrations features)
        {
            return new EntityFeature(((IHaveContainerBuilder)features).Builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ProcessingFeature Processing(this NWheelsFeatureRegistrations features)
        {
            return new ProcessingFeature(((IHaveContainerBuilder)features).Builder);
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
            this UIAppEndpointRegistrations<TApp> fluentRegistration, 
            string name = null, 
            string defaultUrl = null,
            bool exposeExceptions = false) 
            where TApp : UidlApplication
        {
            var registration = new WebAppEndpointRegistration(name, typeof(TApp), defaultUrl, exposeExceptions);
            ((IHaveContainerBuilder)fluentRegistration).Builder.RegisterInstance(registration);
            return fluentRegistration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ApiEndpointRegistration<TContract> WithNetworkEndpoint<TContract>(
            this ApiEndpointRegistration<TContract> fluentRegistration,
            string name = null,
            string defaultUrl = null)
            where TContract : class
        {
            var registration = new NetworkApiEndpointRegistration(name, typeof(TContract), defaultUrl);
            ((IHaveContainerBuilder)fluentRegistration).Builder.RegisterInstance(registration);
            return fluentRegistration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ApiEndpointRegistration<TContract> WithHttpApiEndpoint<TContract>(
            this ApiEndpointRegistration<TContract> registration,
            string name = null,
            string defaultListenUrl = null,
            bool exposeExceptions = false)
            where TContract : class
        {
            ((IHaveContainerBuilder)registration).Builder.RegisterInstance(new HttpApiEndpointRegistration(
                name, typeof(TContract), defaultListenUrl, defaultMetadataUrl: null, publishMetadata: false, exposeExceptions: exposeExceptions));

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

        public static DataRepositoryRegistration<TRepo> WithRestEndpoint<TRepo>(
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HostingFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public HostingFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IRegistrationBuilder<TComponent, SimpleActivatorData, SingleRegistrationStyle> RegisterLifecycleComponent<TComponent>()
                where TComponent : class, ILifecycleEventListener
            {
                return _builder
                    .Register(c =>
                        (TComponent)c.Resolve<ComponentAspectFactory>().CreateInheritor(typeof(TComponent))
                    )
                    .As<ILifecycleEventListener>()
                    .SingleInstance();
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
                _builder.Register(ctx => ctx.Resolve<ConfigurationObjectFactory>().CreateService<TSection>()).As<TSection>().SingleInstance();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class CommunicationFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public CommunicationFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void UseHttpBot()
            {
                _builder.RegisterType<HttpBot>().InstancePerDependency();
                _builder.NWheelsFeatures().Logging().RegisterLogger<IHttpBotLogger>();
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterMetaTypeMutation<TContract>(Action<IComponentContext, TypeMetadataCache, TypeMetadataBuilder> onApply) 
                where TContract : class
            {
                _builder.RegisterInstance(new DelegatingMetadataMutation<TContract>(onApply)).As<IMetadataMutation>();
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

            public void RegisterDatabaseInitializationCheck<TRepo>() where TRepo : class, IApplicationDataRepository
            {
                var registration = new DatabaseInitializationCheckRegistration(typeof(TRepo));
                _builder.RegisterInstance(registration).AsSelf();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IRegistrationBuilder<TPopulator, ConcreteReflectionActivatorData, SingleRegistrationStyle> 
                RegisterDataPopulator<TPopulator>() where TPopulator : IDomainContextPopulator
            {
                return _builder.RegisterType<TPopulator>().As<IDomainContextPopulator>().SingleInstance();
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

            public void UseDefaultIdsOfType<TId>()
            {
                _builder.RegisterType<DefaultIdMetadataConvention>()
                    .WithParameter(TypedParameter.From(typeof(TId)))
                    .As<IMetadataConvention>()
                    .SingleInstance()
                    .LastInPipeline();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //public void UseHiloIntegerIdGenerator(int loDigits)
            //{
            //    _builder.RegisterType<HiloGeneratorMetadataConvention>().As<IMetadataConvention>().LastInPipeline();
            //    _builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<HiloIntegerIdGenerator>()
            //        //.WithParameter(TypedParameter.From(loDigits))
            //        .As<HiloIntegerIdGenerator, IPropertyValueGenerator, IDomainContextPopulator>()
            //        .FirstInPipeline()
            //        .SingleInstance();
            //}

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

            public IRegistrationBuilder<TCollection, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterStorageSchemaMigrations<TCollection>()
                where TCollection : SchemaMigrationCollection
            {
                return _builder.RegisterType<TCollection>().As<SchemaMigrationCollection>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterRelationalMappingFineTune<TEntity>(Action<IRelationalMappingFineTune<TEntity>> fineTuneAction)
                where TEntity : class
            {
                var fineTuner = new RelationalMappingFineTuner<TEntity>(fineTuneAction);
                _builder.RegisterInstance<RelationalMappingFineTuner<TEntity>>(fineTuner);
            }

            ////-----------------------------------------------------------------------------------------------------------------------------------------------------

            //public void UseStorageInitializerOnStartup()
            //{
            //    _builder.NWheelsFeatures().Logging().RegisterLogger<DatabaseInitializer.ILogger>();
            //    _builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<DatabaseInitializer>().FirstInPipeline().AsSelf();
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterDatabaseNameResolver<TResolver>()
                where TResolver : class, IDbConnectionStringResolver
            {
                _builder.RegisterType<TResolver>().As<IDbConnectionStringResolver>().SingleInstance();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AuthorizationFeature
        {
            private readonly ContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AuthorizationFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IRegistrationBuilder<TRule, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAnonymousEntityAccessRule<TRule>()
                where TRule : AnonymousEntityAccessRule
            {
                return _builder.RegisterType<TRule>().As<AnonymousEntityAccessRule>();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ProcessingFeature
        {
            private readonly ContainerBuilder _builder;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessingFeature(ContainerBuilder builder)
            {
                _builder = builder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void UseBuiltInWorkflowEngine()
            {
                _builder.NWheelsFeatures().Logging().RegisterLogger<IWorkflowEngineLogger>();
                _builder.NWheelsFeatures().Logging().RegisterLogger<TransientWorkflowReadyQueue.ILogger>();
                //_builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<TransientWorkflowReadyQueue>().As<IWorkflowReadyQueue>();
                _builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<WorkflowEngine>().As<IWorkflowEngine, IWorkflowInstanceContainer>();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void UseTransientReadyQueue()
            {
                _builder.RegisterType<TransientWorkflowReadyQueue>().As<IWorkflowReadyQueue>().SingleInstance();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterWorkflow<TCodeBehind, TDataRepository, TDataEntity>(Func<TDataRepository, IQueryable<TDataEntity>> entitySelector)
                where TCodeBehind : class, IWorkflowCodeBehind
                where TDataRepository : class, IApplicationDataRepository
                where TDataEntity : class, IWorkflowInstanceEntity
            {
                var registration = new WorkflowTypeRegistration<TCodeBehind, TDataRepository, TDataEntity>(entitySelector);
                _builder.RegisterInstance(registration).As<WorkflowTypeRegistration>();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void RegisterStateMachineWorkflow<TState, TTrigger, TCodeBehind, TDataRepository, TDataEntity>(
                Func<TDataRepository, IQueryable<TDataEntity>> entitySelector)
                where TCodeBehind : class, IStateMachineCodeBehind<TState, TTrigger>
                where TDataRepository : class, IApplicationDataRepository
                where TDataEntity : class, IStateMachineInstanceEntity<TState>
            {
                _builder.RegisterType<StateMachineWorkflow<TState, TTrigger, TDataEntity>>().WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IStateMachineCodeBehind<TState, TTrigger>), 
                    (pi, ctx) => ctx.Resolve<TCodeBehind>()));

                _builder.NWheelsFeatures().Logging().RegisterLogger<TransientStateMachine<TState, TTrigger>.ILogger>();

                RegisterWorkflow<StateMachineWorkflow<TState, TTrigger, TDataEntity>, TDataRepository, TDataEntity>(entitySelector);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IRegistrationBuilder<TActor, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterActor<TActor>()
            {
                var messageHandlerInterfaces = ServiceBus.GetMessageHandlerInterfaces(typeof(TActor)).ToArray();

                foreach ( var handlerInterface in messageHandlerInterfaces )
                {
                    var messageType = handlerInterface.GetGenericArguments()[0];
                    var adapterClosedType = typeof(MessageHandlerAdapter<>).MakeGenericType(messageType);

                    _builder.RegisterType(adapterClosedType).As<IMessageHandlerAdapter>();
                }

                return _builder.RegisterType<TActor>().As(messageHandlerInterfaces);
            }

            ////-----------------------------------------------------------------------------------------------------------------------------------------------------

            //public IRegistrationBuilder<TScript, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterTransactionScript<TScript>()
            //    where TScript : ITransactionScript
            //{
            //    return _builder.RegisterType<TScript>().As<TScript, ITransactionScript>();
            //}

            ////-----------------------------------------------------------------------------------------------------------------------------------------------------

            //public IRegistrationBuilder<TConcrete, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterTransactionScript<TAbstract, TConcrete>()
            //    where TAbstract : ITransactionScript
            //    where TConcrete : TAbstract
            //{
            //    _builder.Register(c => (TAbstract)c.Resolve<TConcrete>()).As<TAbstract, ITransactionScript>();
            //    return _builder.RegisterType<TConcrete>();
            //}

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IRegistrationBuilder<TScript, SimpleActivatorData, SingleRegistrationStyle> RegisterTransactionScript<TScript>()
                where TScript : ITransactionScript
            {
                return _builder.Register(c =>
                    (TScript)c.Resolve<ComponentAspectFactory>().CreateInheritor(typeof(TScript))
                ).As<TScript, ITransactionScript>();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IRegistrationBuilder<TConcrete, SimpleActivatorData, SingleRegistrationStyle> RegisterTransactionScript<TAbstract, TConcrete>()
                where TAbstract : ITransactionScript
                where TConcrete : TAbstract
            {
                _builder.Register<TAbstract>(c =>
                    (TAbstract)c.Resolve<ComponentAspectFactory>().CreateInheritor(typeof(TConcrete))
                ).As<TAbstract, ITransactionScript>();

                return _builder.Register<TConcrete>(c =>
                    (TConcrete)c.Resolve<ComponentAspectFactory>().CreateInheritor(typeof(TConcrete))
                ).As<TConcrete>();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessingFeature RegisterMessage<TBody>()
            {
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody)));
                return this;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessingFeature RegisterMessage<TBody1, TBody2>()
            {
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody1)));
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody2)));
                return this;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessingFeature RegisterMessage<TBody1, TBody2, TBody3>()
            {
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody1)));
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody2)));
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody3)));
                return this;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessingFeature RegisterMessage<TBody1, TBody2, TBody3, TBody4>()
            {
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody1)));
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody2)));
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody3)));
                _builder.RegisterInstance(new MessageTypeRegistration(typeof(TBody4)));
                return this;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsMessageHandlerInterface(Type interfaceType)
            {
                return (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
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

            public UIAppEndpointRegistrations<TApp> RegisterApplication<TApp>() where TApp : UidlApplication
            {
                _builder.RegisterType<TApp>().As<TApp, UidlApplication>();
                return new UIAppEndpointRegistrations<TApp>(_builder);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IRegistrationBuilder<TExtension, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterEntityHandlerExtension<TExtension>() 
                where TExtension : ApplicationEntityService.IEntityHandlerExtension
            {
                return _builder.RegisterType<TExtension>().As<ApplicationEntityService.IEntityHandlerExtension>();
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
                var concretization = 
                    typeof(TConcrete).IsInterface 
                    ? new ConcretizationRegistration(typeof(TGeneral), typeof(TConcrete), domainObject: null)
                    : new ConcretizationRegistration(typeof(TGeneral), typeof(TGeneral), domainObject: typeof(TConcrete));

                _builder.RegisterInstance(concretization).As<ConcretizationRegistration>();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void With<TConcrete, TDomain>() 
                where TConcrete : class, TGeneral
                where TDomain : class, TConcrete
            {
                if ( !typeof(TConcrete).IsInterface || !typeof(TDomain).IsClass )
                {
                    throw new ContractConventionException(
                        "Invalid concretization. Type '{0}' must be an interface and type '{1}' must be a class.", 
                        typeof(TConcrete).FullName, typeof(TDomain).FullName);
                }

                var concretization = new ConcretizationRegistration(typeof(TGeneral), typeof(TConcrete), typeof(TDomain));
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
            where TApp : UidlApplication
        {
            private readonly ContainerBuilder _containerBuilder;
            private readonly Type _applicationType;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public UIAppEndpointRegistrations(ContainerBuilder containerBuilder)
            {
                _applicationType = typeof(TApp);
                _containerBuilder = containerBuilder;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            ContainerBuilder IHaveContainerBuilder.Builder
            {
                get { return _containerBuilder; }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ApplicationType
            {
                get { return _applicationType; }
            }
        }
    }
}
