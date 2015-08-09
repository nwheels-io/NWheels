using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using Hapil;
using NUnit.Framework;
using NWheels.Testing;

namespace NWheels.Stacks.EntityFramework.Tests.Integration
{
    [TestFixture]
    public abstract class DatabaseTestBase : UnitTestBase
    {
        public const string DatabaseName = "NWheelsEFTests";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void DatabaseTestSetUp()
        {
            this.CompiledModel = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void DropAndCreateTestDatabase()
        {
            DropAndCreateSqlServerTestDatabase();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DbConnection CreateDbConnection()
        {
            var dbFactory = DbProviderFactories.GetFactory(this.ConnectionStringProviderName);
            var connection = dbFactory.CreateConnection();
            connection.ConnectionString = this.ConnectionString;
            return connection;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string MasterConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["master"].ConnectionString;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string ConnectionStringProviderName
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["test"].ProviderName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string GetCommaSeparatedColumnList(DataTable table)
        {
            return string.Join(",", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName + ":" + c.DataType.Name).ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataTable SelectFromTable(string tableName)
        {
            var factory = DbProviderFactories.GetFactory(ConnectionStringProviderName);

            using ( var connection = CreateDbConnection() )
            {
                connection.Open();

                var adapter = factory.CreateDataAdapter();
                adapter.SelectCommand = factory.CreateCommand();
                adapter.SelectCommand.CommandText = "SELECT * FROM " + tableName;
                adapter.SelectCommand.Connection = connection;

                var table = new DataTable();
                adapter.Fill(table);
                
                return table;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void CreateTestDatabaseObjects()
        {
            using ( var connection = CreateDbConnection() )
            {
                var objectContext = CompiledModel.CreateObjectContext<ObjectContext>(connection);
                var script = objectContext.CreateDatabaseScript();

                using ( var command = connection.CreateCommand() )
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = script;

                    Console.WriteLine(script);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DbCompiledModel CompiledModel { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DropAndCreateSqlServerTestDatabase()
        {
            using ( var connection = new SqlConnection(this.MasterConnectionString) )
            {
                connection.Open();

                var cmd = new SqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = string.Format(@"
	                IF EXISTS(SELECT * FROM sys.databases WHERE name='{0}')
	                BEGIN
		                ALTER DATABASE [{0}]
		                SET SINGLE_USER
		                WITH ROLLBACK IMMEDIATE
		                DROP DATABASE [{0}]
	                END

	                DECLARE @FILENAME AS VARCHAR(255)

	                SET @FILENAME = CONVERT(VARCHAR(255), SERVERPROPERTY('instancedefaultdatapath')) + '{0}';

	                EXEC ('CREATE DATABASE [{0}] ON PRIMARY 
		                (NAME = [{0}], 
		                FILENAME =''' + @FILENAME + ''', 
		                SIZE = 25MB, 
		                MAXSIZE = 50MB, 
		                FILEGROWTH = 5MB )')",
                    DatabaseName);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
