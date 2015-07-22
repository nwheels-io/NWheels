using Hapil;
using MongoDB.Driver;
using NUnit.Framework;
using NWheels.Entities;
using NWheels.Stacks.MongoDb.Impl;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.Tests.Integration
{
    [TestFixture]
    public class MongoDataRepositoryFactoryTests : IntegrationTestBase
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
        public void CanPerformBasicCrudOperations()
        {
            //-- Arrange

            var factory = CreateDataRepositoryFactory();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(repoFactory: () => factory.CreateService<IR1.IOnlineStoreRepository>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
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
