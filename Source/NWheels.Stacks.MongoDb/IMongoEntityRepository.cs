using System.Collections.Generic;
using MongoDB.Driver;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb
{
    internal interface IMongoEntityRepository
    {
        TEntityContract GetById<TEntityContract>(object id);
        IEnumerable<TEntityContract> GetByIdList<TEntityContract>(System.Collections.IEnumerable idList);
        void CommitInsert(IDomainObject entity);
        void CommitUpdate(IDomainObject entity);
        void CommitSave(IDomainObject entity);
        void CommitDelete(IDomainObject entity);
        void CommitInsert(IEnumerable<IDomainObject> entities);
        void CommitUpdate(IEnumerable<IDomainObject> entities);
        void CommitSave(IEnumerable<IDomainObject> entities);
        void CommitDelete(IEnumerable<IDomainObject> entities);
        MongoCollection GetMongoCollection();
        IEnumerable<TContract> TrackMongoCursor<TContract, TImpl>(MongoCursor<TImpl> cursor);
    }
}
