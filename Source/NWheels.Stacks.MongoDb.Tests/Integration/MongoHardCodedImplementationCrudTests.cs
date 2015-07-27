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
using NWheels.Stacks.MongoDb.Impl;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.Tests.Integration
{
    [TestFixture]
    public class MongoHardCodedImplementationCrudTests : IntegrationTestBase
    {
        public const string TestDatabaseName = "NWheelsHardCodedTest";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Hapil.DynamicModule _dyamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dyamicModule = new DynamicModule(
                "EmittedByMongoHardCodedImplementationCrudTests",
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
            HardCodedImplementations.CurrentDataRepoFactory = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformBasicCrudOperations()
        {
            //-- Arrange

            var repoFactory = CreateDataRepositoryFactory();
            HardCodedImplementations.CurrentDataRepoFactory = repoFactory;

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(repoFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, Ignore("Depends on LINQ-to-aggregation-framework support by mongocsharpdriver")]
        public void CanPerformAdvancedRetrievals()
        {
            //-- Arrange

            var repoFactory = CreateDataRepositoryFactory();
            HardCodedImplementations.CurrentDataRepoFactory = repoFactory;

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

            UpdateHardCodedImplementationTypes(metadataCache);

            var entityFactory = new HardCodedImplementations.HardCodedEntityFactory(base.Framework.Components);

            return () => {
                return new HardCodedImplementations.DataRepositoryObject_OnlineStoreRepository(
                    Framework.Components,
                    entityFactory, 
                    metadataCache, 
                    database, 
                    autoCommit: false);    
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void UpdateHardCodedImplementationTypes(TypeMetadataCache metadataCache)
        {
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IOrder))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityObject_Order));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IOrderLine))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityObject_OrderLine));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IProduct))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityObject_Product));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.ICategory))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityObject_Category));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IAttribute))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityObject_Attribute));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IAttributeValue))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityPartObject_AttributeValue));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IAttributeValueChoice))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityPartObject_AttributeValueChoice));

            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IPostalAddress))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.EntityPartObject_PostalAddress));
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
