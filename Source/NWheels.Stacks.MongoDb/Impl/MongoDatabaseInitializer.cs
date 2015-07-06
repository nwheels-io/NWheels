using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Impl
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

            return server.DatabaseExists(connectionParams.DatabaseName);
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
            
            server.GetDatabase(connectionParams.DatabaseName);
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
    }
}
