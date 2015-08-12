using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Hosting;

namespace NWheels.Testing
{
    public class TestAgentComponent : LifecycleEventListenerBase
    {
        private readonly TestFixtureWithNodeHosts _testFixtureInstance;
        private readonly IComponentContext _components;
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestAgentComponent(IComponentContext components, IFramework framework, TestFixtureWithNodeHosts testFixtureInstance)
        {
            _testFixtureInstance = testFixtureInstance;
            _components = components;
            _framework = framework;
            
            _testFixtureInstance.AgentComponent = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void NodeUnloaded()
        {
            _testFixtureInstance.AgentComponent = null;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContext Components
        {
            get { return _components; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IFramework Framework
        {
            get { return _framework; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterInBootConfig(BootConfiguration bootConfig)
        {
            bootConfig.ApplicationModules.Add(new BootConfiguration.ModuleConfig() {
                Assembly = typeof(AgentModule).Assembly.GetName().Name + ".dll",
                LoaderClass = typeof(AgentModule).FullName
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AgentModule : Autofac.Module
        {
            private readonly TestFixtureWithNodeHosts _testFixtureInstance;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AgentModule(TestFixtureWithNodeHosts testFixtureInstance)
            {
                _testFixtureInstance = testFixtureInstance;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void Load(ContainerBuilder builder)
            {
                builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<TestAgentComponent>();
                _testFixtureInstance.OnRegisteringModuleComponents(builder);
            }
        }
    }
}
