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
    internal class TypedEntityAccessControl<TEntity> : NonTypedEntityAccessControl, ITypedEntityAccessControlBuilder<TEntity>
    {
        private Func<IAccessControlContext, Expression<Func<TEntity, bool>>> _entityQuery;
        private Func<IAccessControlContext, TEntity, bool> _entityRetrieve;
        private Func<IAccessControlContext, TEntity, bool> _entityInsert;
        private Func<IAccessControlContext, TEntity, bool> _entityUpdate;
        private Func<IAccessControlContext, TEntity, bool> _entityDelete;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITypedEntityAccessControlBuilder

        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsFilteredByQuery(
            Func<IAccessControlContext, Expression<Func<TEntity, bool>>> canRetrieveWhere)
        {
            if ( _entityQuery == null )
            {
                _entityQuery = canRetrieveWhere;
                BuildLog.AppendLine("IsFilteredByQuery(...)");
            }
            else
            {
                throw new NotSupportedException("Multiple narrowing queries are not supported within one rule.");
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsDefinedByPredicate(
            Func<IAccessControlContext, TEntity, bool> canRetrieve, 
            Func<IAccessControlContext, TEntity, bool> canInsert, 
            Func<IAccessControlContext, TEntity, bool> canUpdate, 
            Func<IAccessControlContext, TEntity, bool> canDelete)
        {
            BuildLog.AppendFormat(
                "IsDefinedHard(C={0},R={1},U={2},D={3})",
                canInsert != null ? "f(...)" : "null",
                canRetrieve != null ? "f(...)" : "null",
                canUpdate != null ? "f(...)" : "null",
                canDelete != null ? "f(...)" : "null");

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsDenied()
        {
            ((INonTypedEntityAccessControlBuilder)this).IsDenied();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsReadOnly()
        {
            ((INonTypedEntityAccessControlBuilder)this).IsReadOnly();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsDeniedIf(Func<IAccessControlContext, bool> condition)
        {
            ((INonTypedEntityAccessControlBuilder)this).IsDeniedIf(condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsReadOnlyIf(Func<IAccessControlContext, bool> condition)
        {
            ((INonTypedEntityAccessControlBuilder)this).IsReadOnlyIf(condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsDeniedUnless(Func<IAccessControlContext, bool> condition)
        {
            ((INonTypedEntityAccessControlBuilder)this).IsDeniedUnless(condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsReadOnlyUnless(Func<IAccessControlContext, bool> condition)
        {
            ((INonTypedEntityAccessControlBuilder)this).IsReadOnlyUnless(condition);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsDefinedHard(
            bool? canRetrieve, 
            bool? canInsert, 
            bool? canUpdate, 
            bool? canDelete)
        {
            ((INonTypedEntityAccessControlBuilder)this).IsDefinedHard(
                canRetrieve: canRetrieve,
                canInsert: canInsert,
                canUpdate: canUpdate,
                canDelete: canDelete);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsDefinedByContext(
            Func<IAccessControlContext, bool> canRetrieve, 
            Func<IAccessControlContext, bool> canInsert, 
            Func<IAccessControlContext, bool> canUpdate, 
            Func<IAccessControlContext, bool> canDelete)
        {
            ((INonTypedEntityAccessControlBuilder)this).IsDefinedByContext(
                canRetrieve: canRetrieve,
                canInsert: canInsert,
                canUpdate: canUpdate,
                canDelete: canDelete);

            return this;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of NonTypedEntityAccessControl

        public override IQueryable AuthorizeQuery(IAccessControlContext context, IQueryable source)
        {
            var baseSource = base.AuthorizeQuery(context, source);

            if ( _entityQuery != null )
            {
                var filterExpression = _entityQuery(context);
                var typedSource = (IQueryable<TEntity>)baseSource;
                var filteredSource = typedSource.Where(filterExpression);
                return filteredSource;
            }
            else
            {
                return baseSource;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanInsert(IAccessControlContext context)
        {
            if (_entityInsert != null && !HasGlobalInsert)
            {
                return true;
            }

            return base.CanInsert(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanRetrieve(IAccessControlContext context)
        {
            if (_entityRetrieve != null && !HasGlobalRetrieve)
            {
                return true;
            }

            return base.CanRetrieve(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanUpdate(IAccessControlContext context)
        {
            if (_entityUpdate != null && !HasGlobalUpdate)
            {
                return true;
            }

            return base.CanUpdate(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanDelete(IAccessControlContext context)
        {
            if (_entityDelete != null && !HasGlobalDelete)
            {
                return true;
            }

            return base.CanDelete(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanRetrieve(IAccessControlContext context, object entity)
        {
            var canRetrieveResult = base.CanRetrieve(context, entity);

            if ( canRetrieveResult == false )
            {
                return false;
            }

            if ( _entityRetrieve != null )
            {
                return _entityRetrieve(context, (TEntity)entity);
            }

            return canRetrieveResult;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanInsert(IAccessControlContext context, object entity)
        {
            var baseResult = base.CanInsert(context, entity);
            
            if (baseResult == false)
            {
                return false;
            }

            if (_entityInsert != null)
            {
                return _entityInsert(context, (TEntity)entity);
            }

            return baseResult;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanUpdate(IAccessControlContext context, object entity)
        {
            var baseResult = base.CanUpdate(context, entity);

            if (baseResult == false)
            {
                return false;
            }

            if ( _entityUpdate != null )
            {
                return _entityUpdate(context, (TEntity)entity);
            }

            return baseResult;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool? CanDelete(IAccessControlContext context, object entity)
        {
            var baseResult = base.CanDelete(context, entity);

            if (baseResult == false)
            {
                return false;
            }

            if (_entityDelete != null)
            {
                return _entityDelete(context, (TEntity)entity);
            }

            return baseResult;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Func<IAccessControlContext, TEntity, bool> _s_entityTrue = (context, entity) => true;
        private static readonly Func<IAccessControlContext, TEntity, bool> _s_entityFalse = (context, entity) => false;

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
