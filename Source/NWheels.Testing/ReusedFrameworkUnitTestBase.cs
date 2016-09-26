using System;
using Autofac;
using Hapil;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Testing
{
    [TestFixture, Category(TestCategory.Unit)]
    public abstract class ReusedFrameworkUnitTestBase
    {
        private TestFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void BaseFixtureSetUp()
        {
            _framework = new TestFramework(CreateDynamicModule(), this.RegisterModules);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureTearDown]
        public void BaseFixtureTearDown()
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

        protected virtual void RegisterModules(IComponentContext components, ContainerBuilder builder)
        {
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

        protected TestFixtureBase.ITestFixtureBaseLogger Logger
        {
            get
            {
                return _framework.Logger<TestFixtureBase.ITestFixtureBaseLogger>();
            }
        }
    }
}
