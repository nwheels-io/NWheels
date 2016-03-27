using System;
using System.Linq;
using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Driver;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Migrations;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class MongoStorageInitializer : IStorageInitializer
    {
        private readonly Pipeline<IDomainContextPopulator> _populators;
        private readonly IMongoDbLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoStorageInitializer(Pipeline<IDomainContextPopulator> populators, IMongoDbLogger logger)
        {
            _populators = populators;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IStorageInitializer

        public string AlterConnectionString(string originalConnectionString, string newMachineName = null, string newDatabaseName = null)
        {
            var connectionParams = new MongoConnectionStringBuilder(originalConnectionString);

            if ( !string.IsNullOrEmpty(newMachineName) )
            {
                connectionParams.Server = new MongoServerAddress(newMachineName);
            }

            if ( !string.IsNullOrEmpty(newDatabaseName) )
            {
                connectionParams.DatabaseName = newDatabaseName;
            }

            return connectionParams.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

        public void MigrateStorageSchema(string connectionString, DataRepositoryBase context, SchemaMigrationCollection migrations)
        {
            var database = ConnectToDatabase(connectionString);

            using (var activity = _logger.ExecutingMigrationCollection(collectionType: migrations.GetType()))
            {
                try
                {
                    var migrator = new MongoDatabaseMigrator(database, migrations, _logger);
                    migrator.ExecuteMigrations();
                }
                catch (Exception e)
                {
                    activity.Fail(e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CreateStorageSchema(string connectionString)
        {
            var database = ConnectToDatabase(connectionString);
            var migrator = new MongoDatabaseMigrator(database, new EmptySchemaMigrationCollection(), _logger);
            migrator.ExecuteMigrations();
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] ListStorageSchemas(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var allDbNames = server.GetDatabaseNames().ToArray();

            return allDbNames;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static MongoDatabase ConnectToDatabase(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var connectionParams = new MongoConnectionStringBuilder(connectionString);

            var database = server.GetDatabase(connectionParams.DatabaseName);
            return database;
        }
    }
}
