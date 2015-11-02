using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;

namespace NWheels.Authorization.Impl
{
    internal class EntityAccessControl<TEntity> : IEntityAccessControl, IEntityAccessControl<TEntity>, IEntityAccessControlBuilder<TEntity>
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IAccessControlContext, bool> _globalRetrieve = null;
        private Func<IAccessControlContext, bool> _globalInsert = null;
        private Func<IAccessControlContext, bool> _globalUpdate = null;
        private Func<IAccessControlContext, bool> _globalDelete = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IAccessControlContext, Expression<Func<TEntity, bool>>> _entityQuery = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IAccessControlContext, TEntity, bool> _entityRetrieve = null;
        private Func<IAccessControlContext, TEntity, bool> _entityInsert = null;
        private Func<IAccessControlContext, TEntity, bool> _entityUpdate = null;
        private Func<IAccessControlContext, TEntity, bool> _entityDelete = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessControl(ITypeMetadata metaType)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeEntityAccessRule

        public void AuthorizeRetrieve(IAccessControlContext context)
        {
            ValidateOrThrow("Retrieve", CanRetrieve(context));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntity> AuthorizeQuery(IAccessControlContext context, IQueryable source)
        {
            AuthorizeRetrieve(context);

            if ( _entityQuery != null )
            {
                var filterExpression = _entityQuery(context);
                return ((IQueryable<TEntity>)source).Where(filterExpression);
            }
            else
            {
                return (IQueryable<TEntity>)source;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeInsert(IAccessControlContext context, object entity)
        {
            ValidateOrThrow("Insert", CanInsert(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeUpdate(IAccessControlContext context, object entity)
        {
            ValidateOrThrow("Update", CanUpdate(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeDelete(IAccessControlContext context, object entity)
        {
            ValidateOrThrow("Delete", CanDelete(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanRetrieve(IAccessControlContext context)
        {
            if ( _globalRetrieve != null )
            {
                return _globalRetrieve(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanInsert(IAccessControlContext context)
        {
            if ( _globalInsert != null )
            {
                return _globalInsert(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanUpdate(IAccessControlContext context)
        {
            if ( _globalUpdate != null )
            {
                return _globalUpdate(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanDelete(IAccessControlContext context)
        {
            if ( _globalDelete != null )
            {
                return _globalDelete(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanRetrieve(IAccessControlContext context, object entity)
        {
            if ( _entityRetrieve != null )
            {
                return _entityRetrieve(context, (TEntity)entity);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanInsert(IAccessControlContext context, object entity)
        {
            if ( !CanInsert(context).GetValueOrDefault(false) )
            {
                return false;
            }

            if ( _entityInsert != null )
            {
                return _entityInsert(context, (TEntity)entity);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanUpdate(IAccessControlContext context, object entity)
        {
            if ( !CanUpdate(context).GetValueOrDefault(false) )
            {
                return false;
            }

            if ( _entityUpdate != null )
            {
                return _entityUpdate(context, (TEntity)entity);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanDelete(IAccessControlContext context, object entity)
        {
            if ( !CanDelete(context).GetValueOrDefault(false) )
            {
                return false;
            }

            if ( _entityDelete != null )
            {
                return _entityDelete(context, (TEntity)entity);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata MetaType
        {
            get { return _metaType; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeEntityAccessRule

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsDenied()
        {
            _globalRetrieve = _s_globalFalse;
            _globalInsert = _s_globalFalse;
            _globalUpdate = _s_globalFalse;
            _globalDelete = _s_globalFalse;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsReadOnly()
        {
            _globalRetrieve = _s_globalTrue;
            _globalInsert = _s_globalFalse;
            _globalUpdate = _s_globalFalse;
            _globalDelete = _s_globalFalse;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsDeniedIf(Func<IAccessControlContext, bool> condition)
        {
            CombineAndNot(ref _globalRetrieve, condition);
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsReadOnlyIf(Func<IAccessControlContext, bool> condition)
        {
            CombineOr(ref _globalRetrieve, condition);
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsDeniedUnless(Func<IAccessControlContext, bool> condition)
        {
            CombineAnd(ref _globalRetrieve, condition);
            CombineAnd(ref _globalInsert, condition);
            CombineAnd(ref _globalUpdate, condition);
            CombineAnd(ref _globalDelete, condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsReadOnlyUnless(Func<IAccessControlContext, bool> condition)
        {
            _globalRetrieve = _s_globalTrue;
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsDefinedHard(bool? canRetrieve, bool? canInsert, bool? canUpdate, bool? canDelete)
        {
            if ( canRetrieve.HasValue )
            {
                _globalRetrieve = (canRetrieve.Value ? _s_globalTrue : _s_globalFalse);
            }

            if ( canInsert.HasValue )
            {
                _globalInsert = (canInsert.Value ? _s_globalTrue : _s_globalFalse);
            }

            if ( canUpdate.HasValue )
            {
                _globalUpdate = (canUpdate.Value ? _s_globalTrue : _s_globalFalse);
            }

            if ( canDelete.HasValue )
            {
                _globalDelete = (canDelete.Value ? _s_globalTrue : _s_globalFalse);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsFilteredByQuery(
            Func<IAccessControlContext, Expression<Func<TEntity, bool>>> canRetrieveWhere)
        {
            if ( _entityQuery == null )
            {
                _entityQuery = canRetrieveWhere;
            }
            else
            {
                throw new NotSupportedException("Multiple narrowing queries are not supported within one rule.");
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsDefinedByContext(
            Func<IAccessControlContext, bool> canRetrieve, 
            Func<IAccessControlContext, bool> canInsert, 
            Func<IAccessControlContext, bool> canUpdate, 
            Func<IAccessControlContext, bool> canDelete)
        {
            if ( canRetrieve != null )
            {
                CombineAnd(ref _globalRetrieve, canRetrieve);
            }

            if ( canInsert != null )
            {
                CombineAnd(ref _globalInsert, canInsert);
            }

            if ( canUpdate != null )
            {
                CombineAnd(ref _globalUpdate, canUpdate);
            }

            if ( canDelete != null )
            {
                CombineAnd(ref _globalDelete, canDelete);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder<TEntity>.IsDefinedByPredicate(
            Func<IAccessControlContext, TEntity, bool> canRetrieve, 
            Func<IAccessControlContext, TEntity, bool> canInsert, 
            Func<IAccessControlContext, TEntity, bool> canUpdate, 
            Func<IAccessControlContext, TEntity, bool> canDelete)
        {
            if ( canRetrieve != null )
            {
                CombineAnd(ref _entityRetrieve, canRetrieve);
            }

            if ( canInsert != null )
            {
                CombineAnd(ref _entityInsert, canInsert);
            }

            if ( canUpdate != null )
            {
                CombineAnd(ref _entityUpdate, canUpdate);
            }

            if ( canDelete != null )
            {
                CombineAnd(ref _entityDelete, canDelete);
            }

            return this;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateOrThrow(string operation, bool? evaluatedAuthorizarion)
        {
            if ( !evaluatedAuthorizarion.GetValueOrDefault(false) )
            {
                throw new SecurityException(string.Format(
                    "User is not authorized to perform requested operation '{0}' on entity type '{1}'.",
                    operation, this.MetaType.Name));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Func<IAccessControlContext, bool> _s_globalTrue = context => true;
        private static readonly Func<IAccessControlContext, bool> _s_globalFalse = context => false;
        private static readonly Func<IAccessControlContext, TEntity, bool> _s_entityTrue = (context, entity) => true;
        private static readonly Func<IAccessControlContext, TEntity, bool> _s_entityFalse = (context, entity) => false;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CombineOr(ref Func<IAccessControlContext, bool> current, Func<IAccessControlContext, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context) => {
                    return existingFunc(context) || newFunc(context);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CombineOr(ref Func<IAccessControlContext, TEntity, bool> current, Func<IAccessControlContext, TEntity, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context, entity) => {
                    return existingFunc(context, entity) || newFunc(context, entity);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CombineAnd(ref Func<IAccessControlContext, bool> current, Func<IAccessControlContext, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context) => {
                    return existingFunc(context) && newFunc(context);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CombineAnd(ref Func<IAccessControlContext, TEntity, bool> current, Func<IAccessControlContext, TEntity, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context, entity) => {
                    return existingFunc(context, entity) && newFunc(context, entity);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CombineAndNot(ref Func<IAccessControlContext, bool> current, Func<IAccessControlContext, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context) => {
                    return existingFunc(context) && !newFunc(context);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CombineAndNot(ref Func<IAccessControlContext, TEntity, bool> current, Func<IAccessControlContext, TEntity, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context, entity) => {
                    return existingFunc(context, entity) && !newFunc(context, entity);
                };
            }
        }
    }
}
