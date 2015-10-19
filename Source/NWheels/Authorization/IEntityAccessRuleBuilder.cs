using System;
using System.Linq.Expressions;

namespace NWheels.Authorization
{
    public interface IEntityAccessBuilder
    {
        IEntityAccessRuleBuilder<TEntity> ToEntity<TEntity>();
        IEntityAccessRuleBuilder<TEntity> ToEntity<TEntity>(Expression<Func<TEntity, bool>> where);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityAccessRuleBuilder<TEntity>
    {
        IEntityAccessRuleBuilder<TEntity> IsDenied();
        IEntityAccessRuleBuilder<TEntity> IsReadOnly();
        IEntityAccessRuleBuilder<TEntity> IsDeniedIf(Func<IRuntimeAccessContext, bool> condition);
        IEntityAccessRuleBuilder<TEntity> IsReadOnlyIf(Func<IRuntimeAccessContext, bool> condition);
        
        IEntityAccessRuleBuilder<TEntity> IsRestrictedHard(
            bool? canRetrieve = null,
            bool? canInsert = null,
            bool? canUpdate = null,
            bool? canDelete = null);
        
        IEntityAccessRuleBuilder<TEntity> IsNarrowedByQuery(
            Func<IRuntimeAccessContext, Expression<Func<TEntity, bool>>> canRetrieveWhere = null);

        IEntityAccessRuleBuilder<TEntity> IsRestrictedByContext(
            Func<IRuntimeAccessContext, bool> canRetrieve = null,
            Func<IRuntimeAccessContext, bool> canInsert = null,
            Func<IRuntimeAccessContext, bool> canUpdate = null,
            Func<IRuntimeAccessContext, bool> canDelete = null);

        IEntityAccessRuleBuilder<TEntity> IsRestrictedByPredicate(
            Func<IRuntimeAccessContext, TEntity, bool> canRetrieve = null,
            Func<IRuntimeAccessContext, TEntity, bool> canInsert = null,
            Func<IRuntimeAccessContext, TEntity, bool> canUpdate = null,
            Func<IRuntimeAccessContext, TEntity, bool> canDelete = null);
    }
}
