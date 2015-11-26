using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Claims;
using NWheels.Authorization.Core;
using NWheels.Authorization.Impl;
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

        public IAccessControlList GetAccessControlList()
        {
            return new AnonymousAccessControlList();
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

        public bool IsGlobalSystem 
        {
            get { return false; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsGlobalAnonymous 
        {
            get { return true; }
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

        private class AnonymousAccessControlList : IAccessControlList, IEntityAccessControl
        {
            private readonly Claim[] _claims = new Claim[] {
                new UserRoleClaim(AnonymousRole)
            };

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IAccessControlList

            public IEntityAccessControl GetEntityAccessControl(Type entityContractType)
            {
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyCollection<Claim> GetClaims()
            {
                return _claims;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HasClaim(string claimValue)
            {
                return _claims.Any(c => c.Value == claimValue);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IRuntimeEntityAccessRule

            public IQueryable AuthorizeQuery(IAccessControlContext context, IQueryable source)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeRetrieve(IAccessControlContext context)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeInsert(IAccessControlContext context, object entity)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeUpdate(IAccessControlContext context, object entity)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void AuthorizeDelete(IAccessControlContext context, object entity)
            {
                throw AccessDenied();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanRetrieve(IAccessControlContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanInsert(IAccessControlContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanUpdate(IAccessControlContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanDelete(IAccessControlContext context)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanRetrieve(IAccessControlContext context, object entity)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanInsert(IAccessControlContext context, object entity)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanUpdate(IAccessControlContext context, object entity)
            {
                return false;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanDelete(IAccessControlContext context, object entity)
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
