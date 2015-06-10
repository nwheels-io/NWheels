using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.Puzzle.EntityFramework.Conventions;
using NWheels.Puzzle.EntityFramework.Impl;
using NWheels.Testing;
using NWheels.Testing.Entities.Puzzle;
using IR1 = NWheels.Testing.Entities.Puzzle.Interfaces.Repository1;
using HR1 = NWheels.Puzzle.EntityFramework.ComponentTests.HardCodedImplementations.Repository1;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture, Category("Integration")]
    public class EfDataRepositoryFactoryTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dyamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dyamicModule = new DynamicModule(
                "EmittedByEfDataRepositoryFactoryTests", 
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
                repoFactory.NewUnitOfWork<IR1.IOnlineStoreRepository>(autoCommit: true);
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
            var configAuto = ResolveAuto<IFrameworkDatabaseConfig>();
            configAuto.Instance.ConnectionString = ConnectionString;

            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions();
            var entityFactory = new EntityObjectFactory(Framework.Components, _dyamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(_dyamicModule, entityFactory, metadataCache, SqlClientFactory.Instance, configAuto);
            
            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Interfaces.Repository1.IOnlineStoreRepository InitializeDataRepository()
        {
            var repoFactory = CreateDataRepositoryFactory();

            var connection = base.CreateDbConnection();
            var repo = repoFactory.NewUnitOfWork<IR1.IOnlineStoreRepository>(autoCommit: true);

            base.CompiledModel = ((EfDataRepositoryBase)repo).CompiledModel;

            return (IR1.IOnlineStoreRepository)repo;
        }
    }
}
