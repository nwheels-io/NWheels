using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using HR1 = NWheels.Puzzle.EntityFramework.ComponentTests.HardCodedImplementations.Repository1;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
    //[Ignore("Not unit tests")]
    public class HardCodedImplementationTests : DatabaseTestBase
    {
        private DbCompiledModel _compiledModel = null;
        //private Autofac.ContainerBuilder _componentsBuilder = null;
        //private Autofac.IContainer _components = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            //_componentsBuilder = new ContainerBuilder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBuildModelAndInitializeObjectContext()
        {
            EnsureDbCompiledModel();

            using ( var connection = CreateDbConnection() )
            {
                var objectContext = _compiledModel.CreateObjectContext<ObjectContext>(connection);
                var productObjectSet = objectContext.CreateObjectSet<HR1.EntityObject_Product>();
                var orderObjectSet = objectContext.CreateObjectSet<HR1.EntityObject_Order>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanObtainDatabaseScript()
        {
            EnsureDbCompiledModel();

            using ( var connection = CreateDbConnection() )
            {
                var objectContext = _compiledModel.CreateObjectContext<ObjectContext>(connection);
                var script = objectContext.CreateDatabaseScript();

                Assert.That(string.IsNullOrEmpty(script), Is.False);
                Console.WriteLine(script);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateDatabaseObjects()
        {
            //-- Arrange

            EnsureDbCompiledModel();
            DropAndCreateTestDatabase();

            //-- Act
            
            CreateTestDatabaseObjects();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInitializeHardCodedDataRepository()
        {
            //-- Arrange

            DropAndCreateTestDatabase();

            //-- Act

            InitializeHardCodedDataRepository(out _compiledModel).Dispose();
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

        [Test, Ignore("WIP")]
        public void CanFineTuneEntityConfiguration()
        {
            //-- Arrange

            DropAndCreateTestDatabase();

            //-- Act

            //HR1.RegisterEntityFineTunings(_componentsBuilder);
            InitializeHardCodedDataRepository(out _compiledModel).Dispose();
            CreateTestDatabaseObjects();

            //-- Assert

            var productsTable = SelectFromTable("MY_PRODUCTS");
            var ordersTable = SelectFromTable("MY_ORDERS");
            var orderLinesTable = SelectFromTable("MY_ORDER_LINES");

            Assert.That(
                GetCommaSeparatedColumnList(productsTable), 
                Is.EqualTo("Id:Int32,Name:String,MY_SPECIAL_PRICE_COLUMN:Decimal"));
            
            Assert.That(
                GetCommaSeparatedColumnList(ordersTable),
                Is.EqualTo("MY_SPECIAL_ORDER_ID_COLUMN:Int32,PlacedAt:DateTime,Status:Int32"));
            
            Assert.That(
                GetCommaSeparatedColumnList(orderLinesTable),
                Is.EqualTo("Id:Int32,Quantity:Int32,OrderId:Int32,MY_SPECIAL_PRODUCT_ID_COLUMN:Int32"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformBasicCrudOperationsOnHardCodedRepository()
        {
            //-- Arrange

            DropAndCreateTestDatabase();
            InitializeHardCodedDataRepository(out _compiledModel).Dispose();
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(repoFactory: InitializeHardCodedDataRepository);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformAdvancedRetrievalsOnHardCodedRepository()
        {
            //-- Arrange

            DropAndCreateTestDatabase();
            InitializeHardCodedDataRepository(out _compiledModel).Dispose();
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(repoFactory: InitializeHardCodedDataRepository);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetCommaSeparatedColumnList(DataTable table)
        {
            return string.Join(",", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName + ":" + c.DataType.Name).ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DataTable SelectFromTable(string tableName)
        {
            using ( var connection = (SqlConnection)CreateDbConnection() )
            {
                connection.Open();

                var adapter = new SqlDataAdapter("SELECT * FROM " + tableName, connection);
                var table = new DataTable();
                adapter.Fill(table);
                
                return table;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Interfaces.Repository1.IOnlineStoreRepository InitializeHardCodedDataRepository()
        {
            DbCompiledModel compiledModel;
            return InitializeHardCodedDataRepository(out compiledModel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Interfaces.Repository1.IOnlineStoreRepository InitializeHardCodedDataRepository(out DbCompiledModel compiledModel)
        {
            //_components = _componentsBuilder.Build();
            
            var connection = CreateDbConnection();
            connection.Open();
            var repo = new HR1.DataRepositoryObject_DataRepository(connection, autoCommit: false);
            compiledModel = repo.CompiledModel;
            return repo;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateTestDatabaseObjects()
        {
            using ( var connection = CreateDbConnection() )
            {
                var objectContext = _compiledModel.CreateObjectContext<ObjectContext>(connection);
                var script = objectContext.CreateDatabaseScript();

                using ( var command = connection.CreateCommand() )
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = script;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnsureDbCompiledModel()
        {
            if ( _compiledModel != null )
            {
                return;
            }

            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<HR1.EntityObject_Product>().HasEntitySetName("Product");
            modelBuilder.Entity<HR1.EntityObject_Order>().HasEntitySetName("Order");
            modelBuilder.Entity<HR1.EntityObject_OrderLine>().HasEntitySetName("OrderLine");

            var dbFactory = DbProviderFactories.GetFactory(base.ConnectionStringProviderName);

            using ( var connection = dbFactory.CreateConnection() )
            {
                connection.ConnectionString = base.ConnectionString;
                var model = modelBuilder.Build(connection);
                _compiledModel = model.Compile();
            }
        }
    }
}
