using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Core.Configuration;
using NWheels.Core.Conventions;
using NWheels.Core.Logging;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Configuration;
using NWheels.Conventions;

namespace NWheels.Testing
{
    public class TestFramework : IFramework
    {
        private readonly IContainer _components;
        private readonly DynamicModule _dynamicModule;
        private readonly TestThreadLogAppender _logAppender;
        private readonly LoggerObjectFactory _loggerFactory;
        private readonly ConfigurationSectionFactory _configurationFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestFramework()
            : this(s_DefaultDynamicModule)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestFramework(DynamicModule dynamicModule)
        {
            _dynamicModule = dynamicModule;
            _logAppender = new TestThreadLogAppender();
            _loggerFactory = new LoggerObjectFactory(_dynamicModule, _logAppender);
            
            _components = BuildComponentContainer();
            _configurationFactory = new ConfigurationSectionFactory(_components, _dynamicModule);

            this.PresetGuids = new Queue<Guid>();
            this.PresetRandomInt32 = new Queue<int>();
            this.PresetRandomInt64 = new Queue<long>();
            this.NodeConfiguration = new NodeConfiguration {
                ApplicationName = "TEST-APP",
                NodeName = "TEST-NODE",
                InstanceId = "TEST-INSTANCE",
                EnvironmentName = "TEST-ENV",
                EnvironmentType = "TEST-ENV-TYPE"
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit = true) 
            where TRepository : class, Entities.IApplicationDataRepository
        {
            throw new NotImplementedException();
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

        public Queue<Guid> PresetGuids { get; private set; }
        public Queue<int> PresetRandomInt32 { get; private set; }
        public Queue<long> PresetRandomInt64 { get; private set; }
        public Guid? PresetCorrelationId { get; set; }
        public DateTime? PresetUtcNow { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeConfiguration NodeConfiguration { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private IContainer BuildComponentContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ThreadRegistry>().SingleInstance();
            builder.RegisterGeneric(typeof(Auto<>)).SingleInstance();
            builder.RegisterInstance(_loggerFactory).As<IAutoObjectFactory>();

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DynamicModule s_DefaultDynamicModule = new DynamicModule(
            simpleName: "UnitTestDynamicTypes." + Guid.NewGuid().ToString("N"),
            allowSave: false);
    }
}
