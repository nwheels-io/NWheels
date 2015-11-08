using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;

namespace NWheels.Authorization.Impl
{
    internal class EntityAccessControlPipe : IEntityAccessControl
    {
        private IEntityAccessControl[] _sinks;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessControlPipe(IEnumerable<IEntityAccessControl> sinks)
        {
            _sinks = sinks.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeEntityAccessRule

        public IQueryable AuthorizeQuery(IAccessControlContext context, IQueryable source)
        {
            var authorizedQuery = source;

            for ( int i = 0 ; i < _sinks.Length ; i++ )
            {
                authorizedQuery = _sinks[i].AuthorizeQuery(context, authorizedQuery);
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
}
