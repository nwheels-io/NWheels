using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Authorization.Impl;
using NWheels.Concurrency;
using NWheels.Concurrency.Core;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Configuration;
using NWheels.Configuration.Core;
using NWheels.Conventions;
using NWheels.Conventions.Core;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Endpoints;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Logging.Core;
using NWheels.Processing.Commands.Factories;
using NWheels.Processing.Messages;
using NWheels.Testing.Entities.Impl;
using NWheels.Testing.Processing.Messages;

namespace NWheels.Testing
{
    public class TestFramework : IFramework, ICoreFramework
    {
        private readonly IContainer _components;
        private readonly DynamicModule _dynamicModule;
        private readonly TestThreadLogAppender _logAppender;
        private readonly LoggerObjectFactory _loggerFactory;
        private readonly ConfigurationObjectFactory _configurationFactory;
        private readonly UnitOfWorkFactory _unitOfWorkFactory;
        //private readonly TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestFramework()
            : this(_s_defaultDynamicModule)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestFramework(DynamicModule dynamicModule)
        {
            this.PresetGuids = new Queue<Guid>();
            this.PresetRandomInt32 = new Queue<int>();
            this.PresetRandomInt64 = new Queue<long>();
            this.NodeConfiguration = new BootConfiguration {
                ApplicationName = "TEST-APP",
                NodeName = "TEST-NODE",
                InstanceId = "TEST-INSTANCE",
                EnvironmentName = "TEST-ENV",
                EnvironmentType = "TEST-ENV-TYPE"
            };

            //_metadataCache = CreateMetadataCacheWithDefaultConventions();
            _dynamicModule = dynamicModule;
            _logAppender = new TestThreadLogAppender(this);
            _loggerFactory = new LoggerObjectFactory(_dynamicModule, _logAppender);

            BuildComponentContainer(out _components);

            _configurationFactory = _components.Resolve<ConfigurationObjectFactory>();
            _unitOfWorkFactory = new UnitOfWorkFactory(_components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T NewDomainObject<T>() where T : class
        {
            var entityObjectFactory = _components.Resolve<IEntityObjectFactory>();
            var persistableObject = entityObjectFactory.NewEntity<T>();
            return _components.Resolve<IDomainObjectFactory>().CreateDomainObjectInstance<T>(persistableObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit = true, IsolationLevel? isolationLevel = null) 
            where TRepository : class, IApplicationDataRepository
        {
            return _unitOfWorkFactory.NewUnitOfWork<TRepository>(autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IApplicationDataRepository ICoreFramework.NewUnitOfWork(Type repositoryContractType, bool autoCommit, IsolationLevel? isolationLevel)
        {
            return _unitOfWorkFactory.NewUnitOfWork(repositoryContractType, autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResourceLock NewLock(ResourceLockMode mode, string resourceNameFormat, params object[] formatArgs)
        {
            return new ResourceLock(mode, resourceNameFormat.FormatIf(formatArgs));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITimeoutHandle NewTimer(
            string timerName,
            string timerInstanceId,
            TimeSpan initialDueTime,
            Action callback)
        {
            return new TestTimeout<object>(
                this, 
                timerName, 
                timerInstanceId, 
                initialDueTime, 
                callback: obj => {
                    callback();
                }, 
                parameter: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITimeoutHandle NewTimer<TParam>(
            string timerName, 
            string timerInstanceId, 
            TimeSpan initialDueTime, 
            Action<TParam> callback, 
            TParam parameter)
        {
            return new TestTimeout<TParam>(this, timerName, timerInstanceId, initialDueTime, callback, parameter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid NewGuid()
        {
            return (PresetGuids.Count > 0 ? PresetGuids.Dequeue() : Guid.NewGuid());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int NewRandomInt32()
        {
            return (PresetRandomInt32.Count > 0 ? PresetRandomInt32.Dequeue() : 123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long NewRandomInt64()
        {
            return (PresetRandomInt64.Count > 0 ? PresetRandomInt64.Dequeue() : 123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INodeConfiguration CurrentNode
        {
            get
            {
                return this.NodeConfiguration;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IIdentityInfo CurrentIdentity 
        {
            get
            {
                return (
                    PresetIdentity ?? 
                    (Thread.CurrentPrincipal != null ? Thread.CurrentPrincipal.Identity as IIdentityInfo : null));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CurrentSessionId
        {
            get
            {
                return PresetSessionId;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CurrentCorrelationId
        {
            get
            {
                return PresetCorrelationId.GetValueOrDefault(Guid.Empty);
            }
            set
            {
                PresetCorrelationId = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime UtcNow
        {
            get
            {
                return PresetUtcNow.GetValueOrDefault(DateTime.UtcNow);
            }
            set
            {
                PresetUtcNow = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWorkForEntity(Type entityContractType, bool autoCommit = true, IsolationLevel? isolationLevel = null)
        {
            var dataRepositoryFactory = _components.Resolve<IDataRepositoryFactory>();
            var dataRepositoryContract = dataRepositoryFactory.GetDataRepositoryContract(entityContractType);

            return _unitOfWorkFactory.NewUnitOfWork(dataRepositoryContract, autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContext Components
        {
            get
            {
                return _components;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TLogger Logger<TLogger>() where TLogger : class, IApplicationEventLogger
        {
            return _loggerFactory.CreateService<TLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Auto<TLogger> LoggerAuto<TLogger>() where TLogger : class, IApplicationEventLogger
        {
            return new Auto<TLogger>(_loggerFactory.CreateService<TLogger>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TSection ConfigSection<TSection>() where TSection : class, IConfigurationSection
        {
            return _components.Resolve<TSection>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] GetLog()
        {
            return _logAppender.GetLog();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] TakeLog()
        {
            return _logAppender.TakeLog();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetLogStrings()
        {
            return _logAppender.GetLogStrings();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] TakeLogStrings()
        {
            return _logAppender.TakeLogStrings();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UpdateComponents(Action<ContainerBuilder> onRegisterComponents)
        {
            var builder = new ContainerBuilder();
            onRegisterComponents(builder);
            builder.Update(_components.ComponentRegistry);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RebuildMetadataCache(
            IMetadataConvention[] customMetadataConventions = null,
            MixinRegistration[] mixinRegistrations = null,
            ConcretizationRegistration[] concretizationRegistrations = null,
            IRelationalMappingConvention[] relationalMappingConventions = null)
        {
            var metadataCache = CreateMetadataCacheWithDefaultConventions(
                _components,
                _components.Resolve<IEnumerable<IMetadataConvention>>().ConcatIf(customMetadataConventions).ToArray(),
                _components.Resolve<IEnumerable<MixinRegistration>>().ConcatIf(mixinRegistrations).ToArray());
            
            UpdateComponents(builder => builder.RegisterInstance(metadataCache).As<ITypeMetadataCache, TypeMetadataCache>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IIdentityInfo PresetIdentity { get; set; }
        public string PresetSessionId { get; set; }
        public Queue<Guid> PresetGuids { get; private set; }
        public Queue<int> PresetRandomInt32 { get; private set; }
        public Queue<long> PresetRandomInt64 { get; private set; }
        public Guid? PresetCorrelationId { get; set; }
        public DateTime? PresetUtcNow { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BootConfiguration NodeConfiguration { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadataCache MetadataCache
        {
            get
            {
                return _components.Resolve<ITypeMetadataCache>() /*_metadataCache*/;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestServiceBus ServiceBus
        {
            get
            {
                return (TestServiceBus)_components.Resolve<IServiceBus>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void BuildComponentContainer(out IContainer container)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(this).As<IFramework>();
            builder.RegisterInstance(_dynamicModule).As<DynamicModule>();
            builder.RegisterType<ThreadRegistry>().SingleInstance();
            builder.RegisterGeneric(typeof(Auto<>)).SingleInstance();
            builder.RegisterType<UniversalThreadLogAnchor>().As<IThreadLogAnchor>().SingleInstance();

            builder.RegisterInstance(_loggerFactory).As<LoggerObjectFactory, IAutoObjectFactory>();
            builder.RegisterType<ConfigurationObjectFactory>().As<IAutoObjectFactory, IConfigurationObjectFactory, ConfigurationObjectFactory>().SingleInstance();
            builder.RegisterType<TestEntityObjectFactory>().As<IEntityObjectFactory, EntityObjectFactory, TestEntityObjectFactory>().SingleInstance();
            builder.RegisterType<TestDataRepositoryFactory>().As<TestDataRepositoryFactory, IDataRepositoryFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<DomainObjectFactory>().As<IDomainObjectFactory>().SingleInstance();
            builder.RegisterType<PresentationObjectFactory>().As<IPresentationObjectFactory>().SingleInstance();
            builder.RegisterType<MethodCallObjectFactory>().As<IMethodCallObjectFactory>().SingleInstance();
            builder.RegisterType<TestIntIdValueGenerator>().SingleInstance();
            builder.RegisterType<TestServiceBus>().As<IServiceBus>().SingleInstance();

            builder.RegisterType<AccessControlListCache>().SingleInstance();
            builder.RegisterType<LocalTransientSessionManager>().As<ISessionManager, ICoreSessionManager>().SingleInstance();
            builder.NWheelsFeatures().Logging().RegisterLogger<IAuthorizationLogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<ISessionEventLogger>();
            
            builder.NWheelsFeatures().Logging().RegisterLogger<IConfigurationLogger>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IDomainContextLogger>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkDatabaseConfig>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkLoggingConfiguration>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkEndpointsConfig>();
            //builder.NWheelsFeatures().Entities().UseDefaultIdsOfType<int>();

            container = builder.Build();

            var metadataCache = CreateMetadataCacheWithDefaultConventions(_components);
            UpdateComponents(builder2 => builder2.RegisterInstance(metadataCache).As<ITypeMetadataCache, TypeMetadataCache>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DynamicModule _s_defaultDynamicModule = new DynamicModule(
            simpleName: "UnitTestDynamicTypes." + Guid.NewGuid().ToString("N"),
            allowSave: false);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeMetadataCache CreateMetadataCacheWithDefaultConventions(IComponentContext components, params MixinRegistration[] mixinRegistrations)
        {
            return CreateMetadataCacheWithDefaultConventions(
                components,
                new IMetadataConvention[] {
                    new DefaultIdMetadataConvention(typeof(int)), 
                    new IntIdGeneratorMetadataConvention()
                }, 
                mixinRegistrations);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeMetadataCache CreateMetadataCacheWithDefaultConventions(
            IComponentContext components,
            IMetadataConvention[] customMetadataConventions, 
            MixinRegistration[] mixinRegistrations = null,
            ConcretizationRegistration[] concretizationRegistrations = null,
            IRelationalMappingConvention[] relationalMappingConventions = null)
        {
            var metadataConventions = 
                new IMetadataConvention[] {
                    new ContractMetadataConvention(), 
                    new AttributeMetadataConvention(), 
                    new RelationMetadataConvention(), 
                }
                .Concat(customMetadataConventions)
                .ToArray();

            var effectiveRelationalMappingConventions = relationalMappingConventions ?? new IRelationalMappingConvention[] {
                new PascalCaseRelationalMappingConvention(usePluralTableNames: true)
            };

            return CreateMetadataCache(
                components,
                metadataConventions,
                effectiveRelationalMappingConventions, 
                mixinRegistrations ?? new MixinRegistration[0],
                concretizationRegistrations ?? new ConcretizationRegistration[0]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeMetadataCache CreateMetadataCache(
            IComponentContext components,
            IMetadataConvention[] metadataConventions,
            IRelationalMappingConvention[] relationalMappingConventions,
            MixinRegistration[] mixinRegistrations,
            ConcretizationRegistration[] concretizationRegistrations)
        {
            var updater = new ContainerBuilder();

            if ( mixinRegistrations != null )
            {
                foreach ( var mixin in mixinRegistrations )
                {
                    updater.RegisterInstance(mixin).As<MixinRegistration>();
                }
            }

            if ( mixinRegistrations != null )
            {
                foreach ( var concretization in concretizationRegistrations )
                {
                    updater.RegisterInstance(concretization).As<ConcretizationRegistration>();
                }
            }

            updater.Update(components.ComponentRegistry);

            var conventionSet = new MetadataConventionSet(metadataConventions, relationalMappingConventions);
            return new TypeMetadataCache(components, conventionSet);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static DynamicModule DefaultDynamicModule
        {
            get
            {
                return _s_defaultDynamicModule;
            }
        }
    }
}
