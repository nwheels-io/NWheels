using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NUnit.Framework;

namespace NWheels.Testing
{
    [TestFixture]
    public class DynamicTypeIntegrationTestBase : IntegrationTestWithoutNodeHosts
    {
        private Hapil.DynamicModule _dynamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void BaseFixtureSetUp()
        {
            _dynamicModule = new DynamicModule(
                "EmittedBy" + this.GetType().Name,
                allowSave: true,
                saveDirectory: TestContext.CurrentContext.TestDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureTearDown]
        public void BaseFixtureTearDown()
        {
            _dynamicModule.SaveAssembly();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DynamicModule DyamicModule
        {
            get { return _dynamicModule; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TestFixtureWithoutNodeHosts

        protected override DynamicModule CreateDynamicModule()
        {
            return _dynamicModule;
        }

        #endregion
    }
}
