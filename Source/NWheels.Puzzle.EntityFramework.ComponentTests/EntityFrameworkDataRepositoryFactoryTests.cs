using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.Puzzle.EntityFramework.Conventions;
using NWheels.Core.Entities;
using IR1 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository1;
using HR1 = NWheels.Puzzle.EntityFramework.ComponentTests.HardCodedImplementations.Repository1;

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
            var repoFactory = CreateDataRepositoryFactory();

            using ( var connection = base.CreateDbConnection() )
            {
                repoFactory.CreateDataRepository<IR1.IOnlineStoreRepository>(connection, autoCommit: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateDatabaseObjects()
        {
            //-- Arrange

            DropAndCreateTestDatabase();

            //-- Act

            InitializeDataRepository().Dispose();
            CreateTestDatabaseObjects();

            //-- Assert

            var productsTable = SelectFromTable("Products");
            var ordersTable = SelectFromTable("Orders");
            var orderLinesTable = SelectFromTable("OrderLines");

            Assert.That(GetCommaSeparatedColumnList(productsTable), Is.EqualTo("Id:Int32,Name:String,Price:Decimal"));
            Assert.That(GetCommaSeparatedColumnList(ordersTable), Is.EqualTo("Id:Int32,PlacedAt:DateTime,Status:Int32"));
            Assert.That(GetCommaSeparatedColumnList(orderLinesTable), Is.EqualTo("Id:Int32,Quantity:Int32,OrderId:Int32,ProductId:Int32"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformBasicCrudOperations()
        {
            //-- Arrange

            DropAndCreateTestDatabase();
            InitializeDataRepository().Dispose();
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(repoFactory: InitializeDataRepository);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformAdvancedRetrievals()
        {
            //-- Arrange

            DropAndCreateTestDatabase();
            InitializeDataRepository().Dispose();
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(repoFactory: InitializeDataRepository);
        }

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
            var connection = CreateDbConnection();
            connection.Open();
            var repo = new HR1.DataRepositoryObject_DataRepository(connection, autoCommit: false);
            base.CompiledModel = repo.CompiledModel;
            return repo;
        }
    }
}
