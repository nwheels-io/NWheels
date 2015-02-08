using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
    public abstract class DatabaseTestBase
    {
        public const string DatabaseName = "NWheelsEFTests";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DropAndCreateTestDatabase()
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string MasterConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["master"].ConnectionString;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ConnectionStringProviderName
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["test"].ProviderName;
            }
        }
    }
}
