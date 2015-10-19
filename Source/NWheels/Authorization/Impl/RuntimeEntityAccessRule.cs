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
    internal class RuntimeEntityAccessRule<TEntity> : IRuntimeEntityAccessRule, IRuntimeEntityAccessRule<TEntity>, IEntityAccessRuleBuilder<TEntity>
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IRuntimeAccessContext, bool> _globalRetrieve = null;
        private Func<IRuntimeAccessContext, bool> _globalInsert = null;
        private Func<IRuntimeAccessContext, bool> _globalUpdate = null;
        private Func<IRuntimeAccessContext, bool> _globalDelete = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IRuntimeAccessContext, Expression<Func<TEntity, bool>>> _entityQuery = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Func<IRuntimeAccessContext, TEntity, bool> _entityRetrieve = null;
        private Func<IRuntimeAccessContext, TEntity, bool> _entityInsert = null;
        private Func<IRuntimeAccessContext, TEntity, bool> _entityUpdate = null;
        private Func<IRuntimeAccessContext, TEntity, bool> _entityDelete = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuntimeEntityAccessRule(ITypeMetadataCache metadataCache)
        {
            _metaType = metadataCache.GetTypeMetadata(typeof(TEntity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeEntityAccessRule

        public void AuthorizeRetrieve(IRuntimeAccessContext context)
        {
            ValidateOrThrow("Retrieve", CanRetrieve(context));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntity> AuthorizeQuery(IRuntimeAccessContext context, IQueryable<TEntity> source)
        {
            AuthorizeRetrieve(context);

            if ( _entityQuery != null )
            {
                var filterExpression = _entityQuery(context);
                return source.Where(filterExpression);
            }
            else
            {
                return source;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeInsert(IRuntimeAccessContext context, object entity)
        {
            ValidateOrThrow("Insert", CanInsert(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeUpdate(IRuntimeAccessContext context, object entity)
        {
            ValidateOrThrow("Update", CanUpdate(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeDelete(IRuntimeAccessContext context, object entity)
        {
            ValidateOrThrow("Delete", CanDelete(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanRetrieve(IRuntimeAccessContext context)
        {
            if ( _globalRetrieve != null )
            {
                return _globalRetrieve(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanInsert(IRuntimeAccessContext context)
        {
            if ( _globalInsert != null )
            {
                return _globalInsert(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanUpdate(IRuntimeAccessContext context)
        {
            if ( _globalUpdate != null )
            {
                return _globalUpdate(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanDelete(IRuntimeAccessContext context)
        {
            if ( _globalDelete != null )
            {
                return _globalDelete(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanRetrieve(IRuntimeAccessContext context, object entity)
        {
            if ( _entityRetrieve != null )
            {
                return _entityRetrieve(context, (TEntity)entity);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanInsert(IRuntimeAccessContext context, object entity)
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

        public bool? CanUpdate(IRuntimeAccessContext context, object entity)
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

        public bool? CanDelete(IRuntimeAccessContext context, object entity)
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

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsDenied()
        {
            _globalRetrieve = _s_globalFalse;
            _globalInsert = _s_globalFalse;
            _globalUpdate = _s_globalFalse;
            _globalDelete = _s_globalFalse;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsReadOnly()
        {
            _globalRetrieve = _s_globalTrue;
            _globalInsert = _s_globalFalse;
            _globalUpdate = _s_globalFalse;
            _globalDelete = _s_globalFalse;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsDeniedIf(Func<IRuntimeAccessContext, bool> condition)
        {
            CombineAndNot(ref _globalRetrieve, condition);
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsReadOnlyIf(Func<IRuntimeAccessContext, bool> condition)
        {
            CombineOr(ref _globalRetrieve, condition);
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsRestrictedHard(bool? canRetrieve, bool? canInsert, bool? canUpdate, bool? canDelete)
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

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsNarrowedByQuery(
            Func<IRuntimeAccessContext, Expression<Func<TEntity, bool>>> canRetrieveWhere)
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

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsRestrictedByContext(
            Func<IRuntimeAccessContext, bool> canRetrieve, 
            Func<IRuntimeAccessContext, bool> canInsert, 
            Func<IRuntimeAccessContext, bool> canUpdate, 
            Func<IRuntimeAccessContext, bool> canDelete)
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

        IEntityAccessRuleBuilder<TEntity> IEntityAccessRuleBuilder<TEntity>.IsRestrictedByPredicate(
            Func<IRuntimeAccessContext, TEntity, bool> canRetrieve, 
            Func<IRuntimeAccessContext, TEntity, bool> canInsert, 
            Func<IRuntimeAccessContext, TEntity, bool> canUpdate, 
            Func<IRuntimeAccessContext, TEntity, bool> canDelete)
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

        private static readonly Func<IRuntimeAccessContext, bool> _s_globalTrue = context => true;
        private static readonly Func<IRuntimeAccessContext, bool> _s_globalFalse = context => false;
        private static readonly Func<IRuntimeAccessContext, TEntity, bool> _s_entityTrue = (context, entity) => true;
        private static readonly Func<IRuntimeAccessContext, TEntity, bool> _s_entityFalse = (context, entity) => false;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CombineOr(ref Func<IRuntimeAccessContext, bool> current, Func<IRuntimeAccessContext, bool> additional)
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

        private static void CombineOr(ref Func<IRuntimeAccessContext, TEntity, bool> current, Func<IRuntimeAccessContext, TEntity, bool> additional)
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

        private static void CombineAnd(ref Func<IRuntimeAccessContext, bool> current, Func<IRuntimeAccessContext, bool> additional)
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

        private static void CombineAnd(ref Func<IRuntimeAccessContext, TEntity, bool> current, Func<IRuntimeAccessContext, TEntity, bool> additional)
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

        private static void CombineAndNot(ref Func<IRuntimeAccessContext, bool> current, Func<IRuntimeAccessContext, bool> additional)
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

        private static void CombineAndNot(ref Func<IRuntimeAccessContext, TEntity, bool> current, Func<IRuntimeAccessContext, TEntity, bool> additional)
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
