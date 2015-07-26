using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Impl
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
