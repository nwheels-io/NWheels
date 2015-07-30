using System.Collections.Generic;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Factories
{
    internal interface IMongoEntityRepository
    {
        IEntityObject GetById(object id);
        IEnumerable<IEntityObject> GetByIdList(System.Collections.IEnumerable idList);
        void CommitInsert(IEntityObject entity);
        void CommitUpdate(IEntityObject entity);
        void CommitDelete(IEntityObject entity);
        void CommitInsert(IEnumerable<IEntityObject> entities);
        void CommitUpdate(IEnumerable<IEntityObject> entities);
        void CommitDelete(IEnumerable<IEntityObject> entities);
    }
}
