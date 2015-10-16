using System.Collections.Generic;
using MongoDB.Driver;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb
{
    internal interface IMongoEntityRepository
    {
        TEntityContract GetById<TEntityContract>(object id);
        IEnumerable<TEntityContract> GetByIdList<TEntityContract>(System.Collections.IEnumerable idList);
        void CommitInsert(IEntityObject entity);
        void CommitUpdate(IEntityObject entity);
        void CommitDelete(IEntityObject entity);
        void CommitInsert(IEnumerable<IEntityObject> entities);
        void CommitUpdate(IEnumerable<IEntityObject> entities);
        void CommitDelete(IEnumerable<IEntityObject> entities);
        MongoCollection GetMongoCollection();
        IEnumerable<TContract> TrackMongoCursor<TContract, TImpl>(MongoCursor<TImpl> cursor);
    }
}
