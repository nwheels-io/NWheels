using System.Linq;
using System.Linq.Expressions;
using NWheels.DataObjects;

namespace NWheels.Authorization.Core
{
    public interface IEntityAccessControl
    {
        void AuthorizeRetrieve(IAccessControlContext context);
        void AuthorizeInsert(IAccessControlContext context, object entity);
        void AuthorizeUpdate(IAccessControlContext context, object entity);
        void AuthorizeDelete(IAccessControlContext context, object entity);
        bool? CanRetrieve(IAccessControlContext context);
        bool? CanInsert(IAccessControlContext context);
        bool? CanUpdate(IAccessControlContext context);
        bool? CanDelete(IAccessControlContext context);
        bool? CanRetrieve(IAccessControlContext context, object entity);
        bool? CanInsert(IAccessControlContext context, object entity);
        bool? CanUpdate(IAccessControlContext context, object entity);
        bool? CanDelete(IAccessControlContext context, object entity);
        ITypeMetadata MetaType { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityAccessControl<out TEntity> : IEntityAccessControl
    {
        IQueryable<TEntity> AuthorizeQuery(IAccessControlContext context, IQueryable source);
    }
}
