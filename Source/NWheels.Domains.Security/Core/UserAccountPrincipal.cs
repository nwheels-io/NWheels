using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Authorization.Core;

namespace NWheels.Domains.Security.Core
{
    public class UserAccountPrincipal : ClaimsPrincipal, IIdentityInfo
    {
        private readonly IIdentityInfo _identityInfo;

        public UserAccountPrincipal(UserAccountIdentity identity)
            : base(identity)
        {
            _identityInfo = identity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ClaimsPrincipal

        public override bool IsInRole(string role)
        {
            return _identityInfo.GetUserRoles().Contains(role);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new UserAccountIdentity Identity
        {
            get
            {
                return (UserAccountIdentity)base.Identity;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name
        {
            get { return _identityInfo.Name; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AuthenticationType
        {
            get { return _identityInfo.AuthenticationType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAuthenticated
        {
            get { return _identityInfo.IsAuthenticated; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public bool IsOfType(Type accountEntityType)
        {
            return _identityInfo.IsOfType(accountEntityType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string[] GetUserRoles()
        {
            return _identityInfo.GetUserRoles();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IRuntimeEntityAccessRule<TEntity> GetEntityAccessRule<TEntity>()
        {
            return new SystemPrincipal.SystemEntityAccessRule<TEntity>(); //temporarily for compatibility
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UserId
        {
            get { return _identityInfo.UserId; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string LoginName
        {
            get { return _identityInfo.LoginName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string QualifiedLoginName
        {
            get { return _identityInfo.QualifiedLoginName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string PersonFullName
        {
            get { return _identityInfo.PersonFullName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string EmailAddress
        {
            get { return _identityInfo.EmailAddress; }
        }
    }
}
