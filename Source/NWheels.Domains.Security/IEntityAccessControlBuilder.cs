using System;
using System.Linq.Expressions;

namespace NWheels.Domains.Security
{
    public interface IEntityAccessControlBuilder
    {
        IEntityAccessControlRuleBuilder<TEntity> ToEntity<TEntity>();
        IEntityAccessControlRuleBuilder<TEntity> ToEntity<TEntity>(Expression<Func<TEntity, bool>> where);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityAccessControlRuleBuilder<TEntity>
    {
        void IsDenied();
        void IsReadOnly();
        void IsFiltered(Expression<Func<TEntity, bool>> filter);
        void IsByRules(
            bool canInsert, 
            bool canUpdate, 
            bool canDelete);
        void IsByWhereRules(
            Expression<Func<TEntity, bool>> canInsertWhere = null,
            Expression<Func<TEntity, bool>> canUpdateWhere = null,
            Expression<Func<TEntity, bool>> canDeleteWhere = null);
        void IsByPerEntityRules(
            Func<TEntity, bool> canInsert = null,
            Func<TEntity, bool> canUpdate = null,
            Func<TEntity, bool> canDelete = null);
    }
}
