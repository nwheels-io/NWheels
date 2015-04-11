using System;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Testing
{
    [TestFixture]
    public abstract class UnitTestBase
    {
        private TestFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void BaseSetUp()
        {
            _framework = new TestFramework();
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
            return null;
            //return s_Framework.Components.Resolve<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Auto<T> ResolveAuto<T>() where T : class
        {
            return Auto.Of<T>(null);
            //return s_Framework.Components.Resolve<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
        protected TestFramework Framework
        {
            get
            {
                return _framework;
            }
        }
    }
}
