using Autofac;
using Hapil;
using NUnit.Framework;

namespace NWheels.Testing
{
    [TestFixture]
    public abstract class TestFixtureWithoutNodeHosts : TestFixtureBase
    {
        private TestFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void BaseSetUp()
        {
            _framework = new TestFramework(CreateDynamicModule());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TearDown]
        public void BaseTearDown()
        {
            _framework = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected T Resolve<T>() where T : class
        {
            return _framework.Components.Resolve<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Auto<T> ResolveAuto<T>() where T : class
        {
            return _framework.Components.Resolve<Auto<T>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual DynamicModule CreateDynamicModule()
        {
            return TestFramework.DefaultDynamicModule;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
        protected TestFramework Framework
        {
            get
            {
                return _framework;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TestFixtureBase.ITestFixtureBaseLogger Logger
        {
            get
            {
                return _framework.Logger<ITestFixtureBaseLogger>();
            }
        }
    }
}
