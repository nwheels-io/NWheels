using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.Puzzle.EntityFramework.Conventions;
using IR1 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository1;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
    public class EntityFrameworkDataRepositoryFactoryTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dyamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dyamicModule = new DynamicModule(
                "EmittedByEntityFrameworkDataRepositoryFactoryTests", 
                allowSave: true, 
                saveDirectory: TestContext.CurrentContext.TestDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _dyamicModule.SaveAssembly();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateDataRepositoryObject()
        {
            var metadataCache = new TypeMetadataCache();
            var entityFactory = new EfEntityObjectFactory(_dyamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(_dyamicModule, entityFactory, metadataCache);

            using ( var connection = base.CreateDbConnection() )
            {
                repoFactory.CreateDataRepository<IR1.IOnlineStoreRepository>(connection, autoCommit: true);
            }
        }
    }
}
