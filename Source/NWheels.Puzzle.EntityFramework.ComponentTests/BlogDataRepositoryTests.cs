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
using NWheels.Puzzle.EntityFramework.Impl;
using IR2 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository2;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
    public class BlogDataRepositoryTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dynamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dynamicModule = new DynamicModule(
                "EmittedByBlogDataRepositoryTests",
                allowSave: true,
                saveDirectory: TestContext.CurrentContext.TestDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _dynamicModule.SaveAssembly();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateBlogEntitiesMetadata()
        {
            //-- Arrange

            var metadataCache = new TypeMetadataCache(new DataObjectConventions(), new PascalCaseRelationalMappingConvention(usePluralTableNames: true));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateDataRepositoryObject()
        {
            var repoFactory = CreateDataRepositoryFactory();

            using ( var connection = base.CreateDbConnection() )
            {
                repoFactory.CreateDataRepository<IR2.IBlogDataRepository>(connection, autoCommit: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EfDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var metadataCache = new TypeMetadataCache(new DataObjectConventions(), new PascalCaseRelationalMappingConvention(usePluralTableNames: true));
            var entityFactory = new EfEntityObjectFactory(_dynamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(_dynamicModule, entityFactory, metadataCache);
            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IR2.IBlogDataRepository InitializeDataRepository()
        {
            var repoFactory = CreateDataRepositoryFactory();

            var connection = base.CreateDbConnection();
            var repo = repoFactory.CreateDataRepository<IR2.IBlogDataRepository>(connection, autoCommit: true);

            base.CompiledModel = ((EfDataRepositoryBase)repo).CompiledModel;

            return (IR2.IBlogDataRepository)repo;
        }
    }
}
