using System;
using MongoDB.Bson;
using MongoDB.Driver;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class MongoDatabaseInitializer : IStorageInitializer
    {
        private readonly Pipeline<IDataRepositoryPopulator> _populators;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDatabaseInitializer(Pipeline<IDataRepositoryPopulator> populators)
        {
            _populators = populators;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IStorageInitializer

        public bool StorageSchemaExists(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var connectionParams = new MongoConnectionStringBuilder(connectionString);

            if ( !server.DatabaseExists(connectionParams.DatabaseName) )
            {
                return false;
            }

            return server.GetDatabase(connectionParams.DatabaseName).CollectionExists("SystemMigrationLog");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void MigrateStorageSchema(string connectionString)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CreateStorageSchema(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var connectionParams = new MongoConnectionStringBuilder(connectionString);
            
            var database = server.GetDatabase(connectionParams.DatabaseName);

            database.GetCollection<MigrationLogEntry>("SystemMigrationLog").Insert(new MigrationLogEntry() { CurrentVersion = 1 });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropStorageSchema(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var connectionParams = new MongoConnectionStringBuilder(connectionString);

            if ( server.DatabaseExists(connectionParams.DatabaseName) )
            {
                server.DropDatabase(connectionParams.DatabaseName);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MigrationLogEntry
        {
            public int CurrentVersion { get; set; }
        }
    }
}
