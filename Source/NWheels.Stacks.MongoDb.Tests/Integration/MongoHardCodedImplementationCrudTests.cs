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
    public class MongoHardCodedImplementationCrudTests : IntegrationTestWithoutNodeHosts
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

        #region Overrides of TestFixtureWithoutNodeHosts

        protected override DynamicModule CreateDynamicModule()
        {
            return _dyamicModule;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IR1.IOnlineStoreRepository> CreateDataRepositoryFactory()
        {
            var database = new MongoClient().GetServer().GetDatabase(TestDatabaseName);

            var configAuto = ResolveAuto<IFrameworkDatabaseConfig>();
            configAuto.Instance.ConnectionString = string.Format("server=localhost;database={0}", TestDatabaseName);

            Framework.RebuildMetadataCache(new IMetadataConvention[] {
                new DefaultIdMetadataConvention(typeof(ObjectId))
            });

            var metadataCache = (TypeMetadataCache)Framework.MetadataCache;

            UpdateHardCodedImplementationTypes(metadataCache);

            var entityFactory = new HardCodedImplementations.HardCodedEntityFactory(base.Framework.Components);

            return () => {
                return new HardCodedImplementations.MongoDataRepository_OnlineStoreRepository(
                    null,
                    Framework.Components,
                    entityFactory, 
                    metadataCache, 
                    database, 
                    false);    
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void UpdateHardCodedImplementationTypes(TypeMetadataCache metadataCache)
        {
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IOrder))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_Order));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IOrderLine))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_OrderLine));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IProduct))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_Product));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.ICategory))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_Category));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IAttribute))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_Attribute));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IAttributeValue))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_AttributeValue));
            
            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IAttributeValueChoice))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_AttributeValueChoice));

            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IPostalAddress))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_PostalAddress));

            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.ICustomer))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_Customer));

            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IContactDetail))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_ContactDetail));

            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IEmailContactDetail))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_EmailContactDetail));

            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IPhoneContactDetail))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_PhoneContactDetail));

            ((TypeMetadataBuilder)metadataCache.GetTypeMetadata(typeof(IR1.IPostContactDetail))).UpdateImplementation(
                typeof(MongoEntityObjectFactory),
                typeof(HardCodedImplementations.MongoEntityObject_PostContactDetail));
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
