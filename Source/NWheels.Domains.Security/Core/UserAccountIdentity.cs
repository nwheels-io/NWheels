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
        private IReadOnlyDictionary<Type, Claim> _extendedClaimByType;

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

        public T GetClaimOfType<T>() where T : Claim
        {
            return Claims.OfType<T>().FirstOrDefault();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetExtendedClaimByType<T>() where T : Claim
        {
            return (T)_extendedClaimByType[typeof(T)];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetExtendedClaimByType<T>(out T claim) where T : Claim
        {
            Claim nonTypedClaim;

            if ( _extendedClaimByType.TryGetValue(typeof(T), out nonTypedClaim) )
            {
                claim = (T)nonTypedClaim;
                return true;
            }
            else
            {
                claim = null;
                return false;
            }
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
            get { return false; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<Claim> Claims 
        {
            get
            {
                return base.Claims.Concat(_extendedClaimByType.Values);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Claim> ExtendedClaims
        {
            get
            {
                return _extendedClaimByType.Values;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ExtendClaimsOnce(IEnumerable<Claim> extendedClaims)
        {
            if ( _extendedClaimByType != null )
            {
                throw new InvalidOperationException();
            }

            _extendedClaimByType = extendedClaims.ToDictionary(claim => claim.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DoneExtendingClaims()
        {
            if ( _extendedClaimByType == null )
            {
                _extendedClaimByType = new Dictionary<Type, Claim>();
            }
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
