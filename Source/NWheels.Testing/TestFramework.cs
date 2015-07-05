using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
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
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Endpoints;
using NWheels.Entities.Core;
using NWheels.Logging.Core;
using NWheels.Testing.Entities.Impl;

namespace NWheels.Testing
{
    public class TestFramework : IFramework
    {
        private readonly IContainer _components;
        private readonly DynamicModule _dynamicModule;
        private readonly TestThreadLogAppender _logAppender;
        private readonly LoggerObjectFactory _loggerFactory;
        private readonly ConfigurationObjectFactory _configurationFactory;
        private readonly TestDataRepositoryFactory _dataRepositoryFactory;
        private readonly TypeMetadataCache _metadataCache;

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

            _metadataCache = CreateMetadataCacheWithDefaultConventions();
            _dynamicModule = dynamicModule;
            _logAppender = new TestThreadLogAppender(this);
            _loggerFactory = new LoggerObjectFactory(_dynamicModule, _logAppender);

            _components = BuildComponentContainer();
            _configurationFactory = _components.Resolve<ConfigurationObjectFactory>();
            _dataRepositoryFactory = _components.Resolve<TestDataRepositoryFactory>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit = true, IsolationLevel? isolationLevel = null) 
            where TRepository : class, IApplicationDataRepository
        {
            return _dataRepositoryFactory.NewUnitOfWork<TRepository>(autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository NewUnitOfWork(Type repositoryContractType, bool autoCommit = true, IsolationLevel? isolationLevel = null)
        {
            return _dataRepositoryFactory.NewUnitOfWork(repositoryContractType, autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResourceLock NewLock(ResourceLockMode mode, string resourceNameFormat, params object[] formatArgs)
        {
            return new ResourceLock(mode, resourceNameFormat.FormatIf(formatArgs));
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
            return _configurationFactory.CreateService<TSection>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Auto<TSection> ConfigSectionAuto<TSection>() where TSection : class, IConfigurationSection
        {
            return new Auto<TSection>(_configurationFactory.CreateService<TSection>());
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
            get { return _metadataCache; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private IContainer BuildComponentContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(this).As<IFramework>();
            builder.RegisterInstance(_dynamicModule).As<DynamicModule>();
            builder.RegisterType<ThreadRegistry>().SingleInstance();
            builder.RegisterGeneric(typeof(Auto<>)).SingleInstance();
            builder.RegisterInstance(_metadataCache).As<ITypeMetadataCache, TypeMetadataCache>();
            builder.RegisterInstance(_loggerFactory).As<LoggerObjectFactory, IAutoObjectFactory>();
            builder.RegisterType<ConfigurationObjectFactory>().As<IAutoObjectFactory, IConfigurationObjectFactory, ConfigurationObjectFactory>().SingleInstance();
            builder.RegisterType<EntityObjectFactory>().SingleInstance();
            builder.RegisterType<TestDataRepositoryFactory>().As<TestDataRepositoryFactory, IDataRepositoryFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<TestIntIdValueGenerator>().SingleInstance();
            
            builder.NWheelsFeatures().Logging().RegisterLogger<IConfigurationLogger>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkDatabaseConfig>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkLoggingConfiguration>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkEndpointsConfig>();
            builder.NWheelsFeatures().Entities().UseDefaultIdsOfType<int>();

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DynamicModule _s_defaultDynamicModule = new DynamicModule(
            simpleName: "UnitTestDynamicTypes." + Guid.NewGuid().ToString("N"),
            allowSave: false);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeMetadataCache CreateMetadataCacheWithDefaultConventions(params MixinRegistration[] mixinRegistrations)
        {
            return CreateMetadataCacheWithDefaultConventions(new TestIdMetadataConvention(), mixinRegistrations);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeMetadataCache CreateMetadataCacheWithDefaultConventions(
            IMetadataConvention defaultIdConvention, 
            params MixinRegistration[] mixinRegistrations)
        {
            var conventions = new MetadataConventionSet(
                new IMetadataConvention[] {
                    new ContractMetadataConvention(), 
                    new AttributeMetadataConvention(), 
                    new RelationMetadataConvention(), 
                    defaultIdConvention
                },
                new IRelationalMappingConvention[] {
                    new PascalCaseRelationalMappingConvention(usePluralTableNames: true)
                });

            return new TypeMetadataCache(conventions, mixinRegistrations, concretizationRegistrations: new ConcretizationRegistration[0]);
        }
    }
}
