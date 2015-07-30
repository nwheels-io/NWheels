using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.ComponentTests
{
    [TestFixture, Category("Integration")]
    public class MongoDataRepositoryFactoryTests : UnitTestBase
    {
        public const string TestDatabaseName = "NWheelsTest";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Hapil.DynamicModule _dyamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dyamicModule = new DynamicModule(
                "EmittedByMongoDataRepositoryFactoryTests",
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

        [SetUp]
        public void SetUp()
        {
            DropAndCreateTestDatabase();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateDataRepositoryObject()
        {
            //-- arrange
            
            var repoFactory = CreateDataRepositoryFactory();

            //-- act

            repoFactory.NewUnitOfWork<IR1.IOnlineStoreRepository>(autoCommit: true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, Ignore("Requires further enhancements in ")]
        public void CanPerformBasicCrudOperations()
        {
            //-- Arrange

            var factory = CreateDataRepositoryFactory();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(repoFactory: () => factory.CreateService<IR1.IOnlineStoreRepository>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, Ignore("Requires LINQ to Aggregation Framework, waiting for C# Mongo driver v2.1")]
        public void CanPerformAdvancedRetrievals()
        {
            //-- Arrange

            var factory = CreateDataRepositoryFactory();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(repoFactory: () => factory.CreateService<IR1.IOnlineStoreRepository>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MongoDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var configAuto = ResolveAuto<IFrameworkDatabaseConfig>();
            configAuto.Instance.ConnectionString = string.Format("server=localhost;database={0}", TestDatabaseName);

            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions();
            var entityFactory = new MongoEntityObjectFactory(Framework.Components, _dyamicModule, metadataCache);
            var repoFactory = new MongoDataRepositoryFactory(_dyamicModule, entityFactory, metadataCache, configAuto.Instance);
            
            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void DropAndCreateTestDatabase()
        {
            var client = new MongoClient();
            var server = client.GetServer();

            if ( server.DatabaseExists(TestDatabaseName) )
            {
                server.DropDatabase(TestDatabaseName);
            }

            server.GetDatabase(TestDatabaseName);
        }
    }
}
