using System;
using Hapil;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Logging.Core;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.Tests.Integration
{
    [TestFixture]
    public class MongoDataRepositoryFactoryCrudTests : IntegrationTestBase
    {
        public const string TestDatabaseName = "NWheelsMongoCrudTests";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Hapil.DynamicModule _dyamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dyamicModule = new DynamicModule(
                "EmittedByMongoDataRepositoryFactoryCrudTests",
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

        [TearDown]
        public void TearDown()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformBasicCrudOperations()
        {
            //-- Arrange

            var repoFactory = CreateDataRepositoryFactory();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(repoFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, Ignore("Depends on LINQ-to-aggregation-framework support by mongocsharpdriver")]
        public void CanPerformAdvancedRetrievals()
        {
            //-- Arrange

            var repoFactory = CreateDataRepositoryFactory();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(repoFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IR1.IOnlineStoreRepository> CreateDataRepositoryFactory()
        {
            var database = new MongoClient().GetServer().GetDatabase(TestDatabaseName);

            var configAuto = ResolveAuto<IFrameworkDatabaseConfig>();
            configAuto.Instance.ConnectionString = string.Format("server=localhost;database={0}", TestDatabaseName);

            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(new IMetadataConvention[] {
                new DefaultIdMetadataConvention(typeof(ObjectId))
            });

            var entityRepoFactory = new MongoEntityObjectFactory(base.Framework.Components, _dyamicModule, metadataCache);
            var dataRepoFactory = new MongoDataRepositoryFactory(Framework.Components, _dyamicModule, entityRepoFactory, metadataCache, configAuto.Instance);

            return () => {
                return dataRepoFactory.NewUnitOfWork<IR1.IOnlineStoreRepository>(autoCommit: false);
            };
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
