using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Claims;
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

        public IAccessControlList GetAccessControlList()
        {
            return new SystemAccessControlList();
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

        public class SystemAccessControlList : IAccessControlList, IEntityAccessControl
        {
            private readonly Claim[] _claims = new Claim[] {
                new UserRoleClaim(SystemRole)
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

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            #region Implementation of IRuntimeEntityAccessRule

            public IQueryable AuthorizeQuery(IAccessControlContext context, IQueryable source)
            {
                return source;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeRetrieve(IAccessControlContext context)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeInsert(IAccessControlContext context, object entity)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeUpdate(IAccessControlContext context, object entity)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void AuthorizeDelete(IAccessControlContext context, object entity)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanRetrieve(IAccessControlContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanInsert(IAccessControlContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanUpdate(IAccessControlContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanDelete(IAccessControlContext context)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanRetrieve(IAccessControlContext context, object entity)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool? CanInsert(IAccessControlContext context, object entity)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanUpdate(IAccessControlContext context, object entity)
            {
                return true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool? CanDelete(IAccessControlContext context, object entity)
            {
                return true;
            }

            #endregion
        }
    }
}
