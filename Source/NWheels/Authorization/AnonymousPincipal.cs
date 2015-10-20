using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;

namespace NWheels.Authorization
{
    public class AnonymousPrincipal : IPrincipal, IIdentity, IIdentityInfo
    {
        #region Implementation of IPrincipal

        public bool IsOfType(Type accountEntityType)
        {
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IIdentityInfo.IsInRole(string userRole)
        {
            return (userRole == AnonymousRole);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetUserRoles()
        {
            return new[] { AnonymousRole };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IRuntimeEntityAccessRule<TEntity> GetEntityAccessRule<TEntity>()
        {
            return new SystemPrincipal.SystemEntityAccessRule<TEntity>(); //temporarily for compatibility
            //return new AnonymousEntityAccessRule<TEntity>();
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
            return (role == AnonymousRole);
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
            get { return AnonymousRole; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AuthenticationType
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public bool IsAuthenticated
        {
            get { return false; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string AnonymousRole = "anonymous";
        public static readonly AnonymousPrincipal Instance = new AnonymousPrincipal();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AnonymousEntityAccessRule<TEntity> : IRuntimeEntityAccessRule<TEntity>
        {
            #region Implementation of IRuntimeEntityAccessRule

            public IQueryable<TEntity> AuthorizeQuery(IRuntimeAccessContext context, IQueryable<TEntity> source)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeRetrieve(IRuntimeAccessContext context)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeInsert(IRuntimeAccessContext context, object entity)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeUpdate(IRuntimeAccessContext context, object entity)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeDelete(IRuntimeAccessContext context, object entity)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanRetrieve(IRuntimeAccessContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanInsert(IRuntimeAccessContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanUpdate(IRuntimeAccessContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanDelete(IRuntimeAccessContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanRetrieve(IRuntimeAccessContext context, object entity)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanInsert(IRuntimeAccessContext context, object entity)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanUpdate(IRuntimeAccessContext context, object entity)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanDelete(IRuntimeAccessContext context, object entity)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata MetaType
            {
                get { return null; }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private SecurityException AccessDenied()
            {
                return new SecurityException("Anonymous user is not authorized to access data in the system.");
            }
        }
    }
}
