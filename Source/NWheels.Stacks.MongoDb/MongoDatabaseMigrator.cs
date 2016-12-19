using System;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NWheels.Entities.Core;
using NWheels.Entities.Migrations;

namespace NWheels.Stacks.MongoDb
{
    public class MongoDatabaseMigrator
    {
        public const string MigrationLogCollectionName = "SystemMigrationLog";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly string _connectionString;
        private readonly MongoDatabase _db;
        private readonly SchemaMigrationCollection _migrations;
        private readonly IMongoDbLogger _logger;
        private readonly MongoCollection<MigrationLogEntry> _migrationLogCollection;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDatabaseMigrator(string connectionString, MongoDatabase db, SchemaMigrationCollection migrations, IMongoDbLogger logger)
        {
            _connectionString = connectionString;
            _db = db;
            _migrations = migrations;
            _logger = logger;
            _migrationLogCollection = db.GetCollection<MigrationLogEntry>(MigrationLogCollectionName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InitializeMigrationLog(int schemaVersion)
        {
            _migrationLogCollection.Insert(new MigrationLogEntry() {
                Name = "INITIAL DEPLOY",
                Version = schemaVersion,
                ExecutedAtUtc = DateTime.UtcNow
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ExecuteMigrations()
        {
            var lastMigration =  _migrationLogCollection.AsQueryable().OrderByDescending(x => x.Version).FirstOrDefault();
            var currentVersion = (lastMigration != null ? lastMigration.Version : 1);
            var migrationsToExecute = _migrations.GetMigrations().Where(m => m.SchemaVersion > currentVersion).OrderBy(m => m.SchemaVersion).ToArray();

            _logger.MigratingDatabaseSchema(name: _db.Name, dbVersion: currentVersion, appVersion: _migrations.SchemaVersion);

            if (lastMigration == null)
            {
                _migrationLogCollection.Insert(new MigrationLogEntry() {
                    Name = "INITIAL DEPLOY",
                    Version = 1,
                    ExecutedAtUtc = DateTime.UtcNow
                });
            }

            foreach (var migration in migrationsToExecute)
            {
                using (var activity = _logger.ExecutingMigration(version: migration.SchemaVersion, name: migration.GetType().Name))
                {
                    try
                    {
                        ExecuteMigration(migration);
                    }
                    catch (Exception e)
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }

            if (migrationsToExecute.Length > 0)
            {
                _logger.DatabaseMigrationCompleted(name: _db.Name, newVersion: _migrations.SchemaVersion);
            }
            else
            {
                _logger.DatabaseIsUpToDate(name: _db.Name, currentVersion: currentVersion);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteMigration(MigrationBase migration)
        {
            migration.Execute(_connectionString, connectionObject: _db);

            _migrationLogCollection.Insert(new MigrationLogEntry() {
                Name = migration.GetType().Name,
                Version = migration.SchemaVersion,
                ExecutedAtUtc = DateTime.UtcNow
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MigrationLogEntry
        {
            [BsonId]
            public int Version { get; set; }
            public string Name { get; set; }
            public DateTime ExecutedAtUtc { get; set; }
        }
    }
}
