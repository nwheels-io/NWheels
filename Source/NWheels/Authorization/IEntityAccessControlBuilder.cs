using System;
using System.Linq.Expressions;

namespace NWheels.Authorization
{
    public interface IEntityAccessControlBuilder
    {
        IEntityAccessControlBuilder<TEntity> ToEntity<TEntity>();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityAccessControlBuilder<TEntity>
    {
        IEntityAccessControlBuilder<TEntity> IsDenied();
        IEntityAccessControlBuilder<TEntity> IsReadOnly();
        IEntityAccessControlBuilder<TEntity> IsDeniedIf(Func<IAccessControlContext, bool> condition);
        IEntityAccessControlBuilder<TEntity> IsReadOnlyIf(Func<IAccessControlContext, bool> condition);
        IEntityAccessControlBuilder<TEntity> IsDeniedUnless(Func<IAccessControlContext, bool> condition);
        IEntityAccessControlBuilder<TEntity> IsReadOnlyUnless(Func<IAccessControlContext, bool> condition);
        
        IEntityAccessControlBuilder<TEntity> IsDefinedHard(
            bool? canRetrieve = null,
            bool? canInsert = null,
            bool? canUpdate = null,
            bool? canDelete = null);
        
        IEntityAccessControlBuilder<TEntity> IsFilteredByQuery(
            Func<IAccessControlContext, Expression<Func<TEntity, bool>>> canRetrieveWhere = null);

        IEntityAccessControlBuilder<TEntity> IsDefinedByContext(
            Func<IAccessControlContext, bool> canRetrieve = null,
            Func<IAccessControlContext, bool> canInsert = null,
            Func<IAccessControlContext, bool> canUpdate = null,
            Func<IAccessControlContext, bool> canDelete = null);

        IEntityAccessControlBuilder<TEntity> IsDefinedByPredicate(
            Func<IAccessControlContext, TEntity, bool> canRetrieve = null,
            Func<IAccessControlContext, TEntity, bool> canInsert = null,
            Func<IAccessControlContext, TEntity, bool> canUpdate = null,
            Func<IAccessControlContext, TEntity, bool> canDelete = null);
    }
}
