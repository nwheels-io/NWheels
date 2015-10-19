using System.Linq;
using System.Linq.Expressions;
using NWheels.DataObjects;

namespace NWheels.Authorization.Core
{
    public interface IRuntimeEntityAccessRule
    {
        void AuthorizeRetrieve(IRuntimeAccessContext context);
        void AuthorizeInsert(IRuntimeAccessContext context, object entity);
        void AuthorizeUpdate(IRuntimeAccessContext context, object entity);
        void AuthorizeDelete(IRuntimeAccessContext context, object entity);
        bool? CanRetrieve(IRuntimeAccessContext context);
        bool? CanInsert(IRuntimeAccessContext context);
        bool? CanUpdate(IRuntimeAccessContext context);
        bool? CanDelete(IRuntimeAccessContext context);
        bool? CanRetrieve(IRuntimeAccessContext context, object entity);
        bool? CanInsert(IRuntimeAccessContext context, object entity);
        bool? CanUpdate(IRuntimeAccessContext context, object entity);
        bool? CanDelete(IRuntimeAccessContext context, object entity);
        ITypeMetadata MetaType { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IRuntimeEntityAccessRule<TEntity> : IRuntimeEntityAccessRule
    {
        IQueryable<TEntity> AuthorizeQuery(IRuntimeAccessContext context, IQueryable<TEntity> source);
    }
}
