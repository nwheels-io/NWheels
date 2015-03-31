using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Modules.Security
{
    public interface IEntityAccessRule
    {
        ITypeMetadata GetTypeMetadata();
        bool CanQuery();
        bool CanInsert();
        bool CanUpdate();
        bool CanDelete();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityAccessRule<TEntity> : IEntityAccessRule
    {
        IQueryable<TEntity> Filter(IQueryable<TEntity> source);
        bool CanInsert(TEntity entity);
        bool CanUpdate(TEntity entity);
        bool CanDelete(TEntity entity);
    }
}
