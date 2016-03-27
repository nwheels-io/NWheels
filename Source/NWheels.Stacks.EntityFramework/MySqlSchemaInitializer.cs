using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using MySql.Data.MySqlClient;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Migrations;
using NWheels.Stacks.EntityFramework.Factories;

namespace NWheels.Stacks.EntityFramework
{
    public class MySqlSchemaInitializer : IStorageInitializer
    {
        private readonly IFrameworkDatabaseConfig _dbConfig;
        private readonly IEnumerable<DataRepositoryRegistration> _dataRepositoryRegistrations;
        private readonly IDataRepositoryFactory _dataRepositoryFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MySqlSchemaInitializer(
            IFrameworkDatabaseConfig dbConfig, 
            IEnumerable<DataRepositoryRegistration> dataRepositoryRegistrations,
            IDataRepositoryFactory dataRepositoryFactory)
        {
            _dbConfig = dbConfig;
            _dataRepositoryRegistrations = dataRepositoryRegistrations;
            _dataRepositoryFactory = dataRepositoryFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IStorageInitializer

        public string AlterConnectionString(string originalConnectionString, string newMachineName = null, string newDatabaseName = null)
        {
            var connectionParams = new MySqlConnectionStringBuilder(originalConnectionString);

            if ( !string.IsNullOrEmpty(newMachineName) )
            {
                connectionParams.Server = newMachineName;
            }

            if ( !string.IsNullOrEmpty(newDatabaseName) )
            {
                connectionParams.Database = newDatabaseName;
            }

            return connectionParams.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

            var registration = _dataRepositoryRegistrations.FirstOrDefault();

            if ( registration != null )
            {
                using ( var repoInstance = _dataRepositoryFactory.NewUnitOfWork(null, registration.DataRepositoryType, autoCommit: false) )
                {
                    var efDataRepository = repoInstance as EfDataRepositoryBase;

                    if ( efDataRepository != null )
                    {
                        efDataRepository.InitializeCurrentSchema();
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropStorageSchema(string connectionString)
        {
            var stringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var sqlStatement = string.Format("drop schema if exists `{0}`", SanitizeSchemaName(stringBuilder.Database));

            ExecuteMasterSql(sqlStatement);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] ListStorageSchemas(string connectionString)
        {
            throw new NotImplementedException();
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
