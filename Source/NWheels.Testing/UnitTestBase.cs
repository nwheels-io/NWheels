using System;
using Autofac;
using NUnit.Framework;

namespace NWheels.Testing
{
    [TestFixture]
    public abstract class UnitTestBase
    {
        [SetUp]
        public void BaseSetUp()
        {
            s_Framework = new TestFramework();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TearDown]
        public void BaseTearDown()
        {
            s_Framework = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected T Resolve<T>() where T : class
        {
            return null;
            //return s_Framework.Components.Resolve<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TestFramework Framework
        {
            get
            {
                return s_Framework;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static TestFramework s_Framework;
    }
}
