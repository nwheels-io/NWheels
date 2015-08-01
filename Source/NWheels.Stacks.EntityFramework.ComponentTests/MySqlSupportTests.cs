using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Testing.NUnit;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities.Core;
using NWheels.Stacks.EntityFramework.Factories;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;
using HR1 = NWheels.Stacks.EntityFramework.ComponentTests.HardCodedImplementations.Repository1;

namespace NWheels.Stacks.EntityFramework.ComponentTests
{
    [TestFixture, Category("Integration")]
    public class MySqlSupportTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dyamicModule;
        private SqlLoggingInterceptor _loggingInterceptor;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dyamicModule = new DynamicModule(
                "EmittedByMySqlSupportTests", 
                allowSave: true, 
                saveDirectory: TestContext.CurrentContext.TestDirectory);

            _loggingInterceptor = new SqlLoggingInterceptor();
            DbInterception.Add(_loggingInterceptor);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            DbInterception.Remove(_loggingInterceptor);
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

        public override void DropAndCreateTestDatabase()
        {
            DropAndCreateMySqlTestDatabase();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ConnectionStringProviderName
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["test_mysql"].ProviderName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["test_mysql"].ConnectionString;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string MasterConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["master_mysql"].ConnectionString;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EfDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var configAuto = ResolveAuto<IFrameworkDatabaseConfig>();
            configAuto.Instance.ConnectionString = ConnectionString;

            var conventions = new MetadataConventionSet(
                new IMetadataConvention[] { new ContractMetadataConvention(), new AttributeMetadataConvention(), new RelationMetadataConvention() },
                new IRelationalMappingConvention[] { new PascalCaseRelationalMappingConvention(usePluralTableNames: true) });

            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions();
            var entityFactory = new EfEntityObjectFactory(Framework.Components, _dyamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(Framework.Components, _dyamicModule, entityFactory, metadataCache, new MySqlClientFactory(), configAuto);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DropAndCreateMySqlTestDatabase()
        {
            using ( var connection = new MySqlConnection(MasterConnectionString) )
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = string.Format("drop schema if exists `{0}`", DatabaseName);
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = string.Format("create schema `{0}`", DatabaseName);
                command.ExecuteNonQuery();
            }

            using ( var connection = new MySqlConnection(ConnectionString) )
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "Create FUNCTION TruncateTime(dateValue DateTime) RETURNS date\r\nreturn Date(dateValue);\r\n";
                command.ExecuteNonQuery();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SqlLoggingInterceptor : IDbCommandInterceptor
        {
            public void NonQueryExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
            {
            }
            public void NonQueryExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
            {
                Console.Write(command.CommandText);
            }
            public void ReaderExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
            {
            }
            public void ReaderExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
            {
                Console.Write(command.CommandText);
            }
            public void ScalarExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
            {
            }
            public void ScalarExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
            {
                Console.Write(command.CommandText);
            }
        }
    }
}
