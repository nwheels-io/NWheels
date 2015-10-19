using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;

namespace NWheels.Authorization
{
    public class SystemPrincipal : IPrincipal, IIdentity, IIdentityInfo
    {
        #region Implementation of IPrincipal

        public bool IsOfType(Type accountEntityType)
        {
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IIdentityInfo.IsInRole(string userRole)
        {
            return (userRole == SystemRole);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetUserRoles()
        {
            return new[] { SystemRole };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IRuntimeEntityAccessRule<TEntity> GetEntityAccessRule<TEntity>()
        {
            return new SystemEntityAccessRule<TEntity>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UserId
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string LoginName
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string QualifiedLoginName
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string PersonFullName
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string EmailAddress
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IPrincipal.IsInRole(string role)
        {
            return (role == SystemRole);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IIdentity Identity
        {
            get { return this; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IIdentity

        public string Name
        {
            get { return SystemRole; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AuthenticationType
        {
            get { return SystemRole; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public bool IsAuthenticated
        {
            get { return true; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string SystemRole = "SYSTEM";
        public static readonly SystemPrincipal Instance = new SystemPrincipal();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SystemEntityAccessRule<TEntity> : IRuntimeEntityAccessRule<TEntity>
        {
            #region Implementation of IRuntimeEntityAccessRule

            public IQueryable<TEntity> AuthorizeQuery(IRuntimeAccessContext context, IQueryable<TEntity> source)
            {
                return source;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeRetrieve(IRuntimeAccessContext context)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeInsert(IRuntimeAccessContext context, object entity)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeUpdate(IRuntimeAccessContext context, object entity)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeDelete(IRuntimeAccessContext context, object entity)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanRetrieve(IRuntimeAccessContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanInsert(IRuntimeAccessContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanUpdate(IRuntimeAccessContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanDelete(IRuntimeAccessContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanRetrieve(IRuntimeAccessContext context, object entity)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanInsert(IRuntimeAccessContext context, object entity)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanUpdate(IRuntimeAccessContext context, object entity)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanDelete(IRuntimeAccessContext context, object entity)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata MetaType
            {
                get { return null; }
            }

            #endregion
        }
    }
}
