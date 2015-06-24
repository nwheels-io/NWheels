using System;
using MongoDB.Driver;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Impl
{
    public abstract class MongoDataRepositoryBase : DataRepositoryBase
    {
        private readonly MongoDatabase _database;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MongoDataRepositoryBase(bool autoCommit, MongoDatabase database)
            : base(autoCommit)
        {
            _database = database;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnCommitChanges()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnRollbackChanges()
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MongoDatabase Database
        {
            get { return _database; }
        }
    }
}
