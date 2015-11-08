using System;
using System.Linq.Expressions;

namespace NWheels.Authorization
{
    public interface IEntityAccessControlBuilder
    {
        INonTypedEntityAccessControlBuilder ToAllEntities();
        ITypedEntityAccessControlBuilder<T> ToEntity<T>();
        INonTypedEntityAccessControlBuilder ToEntities<T1, T2>();
        INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3>();
        INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4>();
        INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5>();
        INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6>();
        INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7>();
        INonTypedEntityAccessControlBuilder ToEntities<T1, T2, T3, T4, T5, T6, T7, T8>();
        INonTypedEntityAccessControlBuilder ToEntities(params Type[] entityContractTypes);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface INonTypedEntityAccessControlBuilder
    {
        INonTypedEntityAccessControlBuilder IsDenied();
        INonTypedEntityAccessControlBuilder IsReadOnly();
        INonTypedEntityAccessControlBuilder IsDeniedIf(Func<IAccessControlContext, bool> condition);
        INonTypedEntityAccessControlBuilder IsReadOnlyIf(Func<IAccessControlContext, bool> condition);
        INonTypedEntityAccessControlBuilder IsDeniedUnless(Func<IAccessControlContext, bool> condition);
        INonTypedEntityAccessControlBuilder IsReadOnlyUnless(Func<IAccessControlContext, bool> condition);

        INonTypedEntityAccessControlBuilder IsDefinedHard(
            bool? canRetrieve = null,
            bool? canInsert = null,
            bool? canUpdate = null,
            bool? canDelete = null);

        INonTypedEntityAccessControlBuilder IsDefinedByContext(
            Func<IAccessControlContext, bool> canRetrieve = null,
            Func<IAccessControlContext, bool> canInsert = null,
            Func<IAccessControlContext, bool> canUpdate = null,
            Func<IAccessControlContext, bool> canDelete = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITypedEntityAccessControlBuilder<TEntity>
    {
        ITypedEntityAccessControlBuilder<TEntity> IsDenied();
        ITypedEntityAccessControlBuilder<TEntity> IsReadOnly();
        ITypedEntityAccessControlBuilder<TEntity> IsDeniedIf(Func<IAccessControlContext, bool> condition);
        ITypedEntityAccessControlBuilder<TEntity> IsReadOnlyIf(Func<IAccessControlContext, bool> condition);
        ITypedEntityAccessControlBuilder<TEntity> IsDeniedUnless(Func<IAccessControlContext, bool> condition);
        ITypedEntityAccessControlBuilder<TEntity> IsReadOnlyUnless(Func<IAccessControlContext, bool> condition);
        
        ITypedEntityAccessControlBuilder<TEntity> IsDefinedHard(
            bool? canRetrieve = null,
            bool? canInsert = null,
            bool? canUpdate = null,
            bool? canDelete = null);
        
        ITypedEntityAccessControlBuilder<TEntity> IsFilteredByQuery(
            Func<IAccessControlContext, Expression<Func<TEntity, bool>>> canRetrieveWhere = null);

        ITypedEntityAccessControlBuilder<TEntity> IsDefinedByContext(
            Func<IAccessControlContext, bool> canRetrieve = null,
            Func<IAccessControlContext, bool> canInsert = null,
            Func<IAccessControlContext, bool> canUpdate = null,
            Func<IAccessControlContext, bool> canDelete = null);

        ITypedEntityAccessControlBuilder<TEntity> IsDefinedByPredicate(
            Func<IAccessControlContext, TEntity, bool> canRetrieve = null,
            Func<IAccessControlContext, TEntity, bool> canInsert = null,
            Func<IAccessControlContext, TEntity, bool> canUpdate = null,
            Func<IAccessControlContext, TEntity, bool> canDelete = null);
    }
}
