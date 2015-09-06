using System;
using System.Security;
using MySql.Data.MySqlClient;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.EntityFramework
{
    public class MySqlSchemaInitializer : IStorageInitializer
    {
        private readonly IFrameworkDatabaseConfig _dbConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MySqlSchemaInitializer(IFrameworkDatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IStorageInitializer

        public bool StorageSchemaExists(string connectionString)
        {
            var stringBuilder = new MySqlConnectionStringBuilder(connectionString);

            using ( var connection = new MySqlConnection(_dbConfig.MasterConnectionString) )
            {
                connection.Open();

                var sqlStatement = "select schema_name from information_schema.schemata where schema_name = @databaseName";

                using ( var command = new MySqlCommand(sqlStatement, connection) )
                {
                    command.Parameters.AddWithValue("@databaseName", stringBuilder.Database);
                    return command.ExecuteReader().Read();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void MigrateStorageSchema(string connectionString, DataRepositoryBase context, SchemaMigrationCollection migrations)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CreateStorageSchema(string connectionString)
        {
            var stringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var sqlStatement = string.Format("create schema `{0}`", SanitizeSchemaName(stringBuilder.Database));

            ExecuteMasterSql(sqlStatement);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropStorageSchema(string connectionString)
        {
            var stringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var sqlStatement = string.Format("drop schema if exists `{0}`", SanitizeSchemaName(stringBuilder.Database));

            ExecuteMasterSql(sqlStatement);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteMasterSql(string sqlStatement)
        {
            using ( var connection = new MySqlConnection(_dbConfig.MasterConnectionString) )
            {
                connection.Open();

                using ( var command = new MySqlCommand(sqlStatement, connection) )
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string SanitizeSchemaName(string objectName)
        {
            if ( string.IsNullOrWhiteSpace(objectName) || objectName.Contains("`") )
            {
                throw new SecurityException("Suspicious MySql schema name encountered.");
            }

            return objectName;
        }
    }
}
