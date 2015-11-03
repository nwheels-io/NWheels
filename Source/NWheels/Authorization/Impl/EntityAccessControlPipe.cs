using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;

namespace NWheels.Authorization.Impl
{
    internal class EntityAccessControlPipe<TEntity> : IEntityAccessControl<TEntity>
    {
        private IEntityAccessControl<TEntity>[] _sinks;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessControlPipe(IEnumerable<IEntityAccessControl<TEntity>> sinks)
        {
            _sinks = sinks.ToArray();
            this.MetaType = _sinks[0].MetaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeEntityAccessRule

        public IQueryable AuthorizeQuery(IAccessControlContext context, IQueryable<TEntity> source)
        {
            var authorizedQuery = source;

            for ( int i = 0 ; i < _sinks.Length ; i++ )
            {
                authorizedQuery = (IQueryable<TEntity>)_sinks[i].AuthorizeQuery(context, authorizedQuery);
            }

            return authorizedQuery;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeRetrieve(IAccessControlContext context)
        {
            for ( int i = 0; i < _sinks.Length; i++ )
            {
                _sinks[i].AuthorizeRetrieve(context);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeInsert(IAccessControlContext context, object entity)
        {
            for ( int i = 0; i < _sinks.Length; i++ )
            {
                _sinks[i].AuthorizeInsert(context, entity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeUpdate(IAccessControlContext context, object entity)
        {
            for ( int i = 0; i < _sinks.Length; i++ )
            {
                _sinks[i].AuthorizeUpdate(context, entity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeDelete(IAccessControlContext context, object entity)
        {
            for ( int i = 0; i < _sinks.Length; i++ )
            {
                _sinks[i].AuthorizeDelete(context, entity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanRetrieve(IAccessControlContext context)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanRetrieve(context)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanInsert(IAccessControlContext context)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanInsert(context)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanUpdate(IAccessControlContext context)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanUpdate(context)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanDelete(IAccessControlContext context)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanDelete(context)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanRetrieve(IAccessControlContext context, object entity)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanRetrieve(context, entity)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanInsert(IAccessControlContext context, object entity)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanInsert(context, entity)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanUpdate(IAccessControlContext context, object entity)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanUpdate(context, entity)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool? CanDelete(IAccessControlContext context, object entity)
        {
            bool? result = null;

            for ( int i = 0; i < _sinks.Length; i++ )
            {
                if ( !AddUp(ref result, _sinks[i].CanDelete(context, entity)) )
                {
                    return false;
                }
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata MetaType { get; private set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool AddUp(ref bool? sum, bool? delta)
        {
            if ( delta == false )
            {
                return false;
            }

            if ( delta == true )
            {
                sum = true;
            }

            return true;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal static class EntityAccessControlPipe
    {
        public static IEntityAccessControl Create(Type contractType, IEnumerable<IEntityAccessControl> sinks)
        {
            var factoryType = typeof(PipeFactory<>).MakeGenericType(contractType);
            var factory = (PipeFactory)Activator.CreateInstance(factoryType);

            return factory.CreatePipe(sinks);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class PipeFactory
        {
            public abstract IEntityAccessControl CreatePipe(IEnumerable<IEntityAccessControl> sinks);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------

        private class PipeFactory<TEntity> : PipeFactory
        {
            public override IEntityAccessControl CreatePipe(IEnumerable<IEntityAccessControl> sinks)
            {
                return new EntityAccessControlPipe<TEntity>(sinks.Cast<IEntityAccessControl<TEntity>>());
            }
        }
    }
}
