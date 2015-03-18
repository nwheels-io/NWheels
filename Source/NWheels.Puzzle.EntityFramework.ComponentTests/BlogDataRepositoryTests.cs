using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.Core.Entities;
using NWheels.Puzzle.EntityFramework.Conventions;
using IR2 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository2;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
    public class BlogDataRepositoryTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dyamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dyamicModule = new DynamicModule(
                "EmittedByBlogDataRepositoryTests",
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

        //[Test]
        //public void CanCreateDataRepositoryObject()
        //{
        //    var repoFactory = CreateDataRepositoryFactory();

        //    using ( var connection = base.CreateDbConnection() )
        //    {
        //        repoFactory.CreateDataRepository<IR2.>(connection, autoCommit: true);
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EfDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var metadataCache = new TypeMetadataCache(new DataObjectConventions(), new PascalCaseRelationalMappingConvention(usePluralTableNames: true));
            var entityFactory = new EfEntityObjectFactory(_dyamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(_dyamicModule, entityFactory, metadataCache);
            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Interfaces.Repository1.IOnlineStoreRepository InitializeDataRepository()
        {
            //var connection = CreateDbConnection();
            //connection.Open();
            //var repo = new HR1.DataRepositoryObject_DataRepository(connection, autoCommit: false);
            //base.CompiledModel = repo.CompiledModel;
            //return repo;
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

  
    }
}
