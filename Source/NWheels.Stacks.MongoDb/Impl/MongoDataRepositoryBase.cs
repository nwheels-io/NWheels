using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Impl
{
    public abstract class MongoDataRepositoryBase : DataRepositoryBase
    {
        private readonly MongoDatabase _database;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MongoDataRepositoryBase(
            IComponentContext components,
            IEntityObjectFactory objectFactory, 
            object emptyModel, 
            MongoDatabase database, 
            bool autoCommit)
            : base(components, autoCommit)
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
