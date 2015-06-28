using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Stacks.MongoDb.Impl;
using NWheels.Testing;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.ComponentTests
{
    [TestFixture, Category("Integration")]
    public class MongoDataRepositoryFactoryTests : DynamicTypeUnitTestBase
    {
        public const string TestDatabaseName = "NWheelsTest";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            DropAndCreateDatabase();
            
            var config = Framework.ConfigSection<IFrameworkDatabaseConfig>();
            config.ConnectionString = string.Format("mongodb://localhost/{0}", TestDatabaseName);
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

        private MongoDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(new DefaultIdMetadataConvention(typeof(ObjectId)));
            var entityFactory = new EntityObjectFactory(Framework.Components, DyamicModule, metadataCache);
            var repoFactory = new MongoDataRepositoryFactory(DyamicModule, entityFactory, metadataCache, Framework.ConfigSection<IFrameworkDatabaseConfig>());
            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void DropAndCreateDatabase()
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
