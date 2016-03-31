using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Migrations;

namespace NWheels.Entities.Core
{
    public interface IStorageInitializer
    {
        string AlterConnectionString(string originalConnectionString, string newMachineName = null, string newDatabaseName = null);
        bool StorageSchemaExists(string connectionString);
        void MigrateStorageSchema(string connectionString, DataRepositoryBase context, SchemaMigrationCollection migrations);
        void CreateStorageSchema(string connectionString, int schemaVersion);
        void DropStorageSchema(string connectionString);
        string[] ListStorageSchemas(string connectionString);
    }
}
