using System.Linq;
using NWheels.DataObjects;

namespace NWheels.Authorization
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
