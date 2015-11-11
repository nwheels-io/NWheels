using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Extensions;
using NWheels.Authorization;
using NWheels.Authorization.Claims;
using NWheels.Authorization.Core;
using NWheels.Authorization.Impl;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.Security.Core
{
    public class UserAccountIdentity : ClaimsIdentity, IIdentityInfo
    {
        public static readonly string AuthenticationTypeString = "NWheels.Domains.Security";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private readonly IUserAccountEntity _userAccount;
        private readonly IAccessControlList _accessControlList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountIdentity(IUserAccountEntity userAccount, IEnumerable<Claim> claims, IAccessControlList accessControlList)
            : base(claims.SelectMany(ExpandWithImpliedClaims))
        {
            _userAccount = userAccount;
            _accessControlList = accessControlList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IUserAccountEntity GetUserAccount()
        {
            return _userAccount;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool IsAuthenticated
        {
            get { return true; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string AuthenticationType
        {
            get { return AuthenticationTypeString; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return _userAccount.LoginName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IIdentityInfo.IsOfType(Type accountEntityType)
        {
            return accountEntityType.IsAssignableFrom(_userAccount.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IIdentityInfo.IsInRole(string userRole)
        {
            return Claims.Any(c => c.Type == UserRoleClaim.UserRoleClaimTypeString && c.Value == userRole);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string[] IIdentityInfo.GetUserRoles()
        {
            return Claims.Where(c => c.Type == UserRoleClaim.UserRoleClaimTypeString).Select(c => c.Value).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAccessControlList GetAccessControlList()
        {
            return _accessControlList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IIdentityInfo.UserId
        {
            get
            {
                return EntityId.ValueOf(_userAccount).ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IIdentityInfo.LoginName
        {
            get { return _userAccount.LoginName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IIdentityInfo.QualifiedLoginName
        {
            get { return _userAccount.LoginName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IIdentityInfo.PersonFullName
        {
            get { return _userAccount.FullName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IIdentityInfo.EmailAddress
        {
            get
            {
                return _userAccount.EmailAddress;
            }
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

        private static IEnumerable<Claim> ExpandWithImpliedClaims(Claim claim)
        {
            var implyMore = claim as IImplyMoreClaims;

            if ( implyMore != null )
            {
                var moreClaims = implyMore.GetImpliedClaims();
                return new[] { claim }.Concat(moreClaims.SelectMany(ExpandWithImpliedClaims));
            }
            else
            {
                return new[] { claim };
            }
        }
    }
}
