using System.Security;
using System.Security.Permissions;
using NWheels.Authorization.Claims;

namespace NWheels.Authorization
{
    public abstract class ClaimCodeAccessAttribute : CodeAccessSecurityAttribute
    {
        private readonly string _requiredClaimType;
        private readonly string[] _requiredClaimValues;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ClaimCodeAccessAttribute(SecurityAction action, string requiredClaimType, params string[] requiredClaimValues)
            : base(action)
        {
            _requiredClaimType = requiredClaimType;
            _requiredClaimValues = requiredClaimValues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of SecurityAttribute

        public override IPermission CreatePermission()
        {
            return new ClaimsPermission(_requiredClaimType, _requiredClaimValues);
        }

        #endregion
    }
}
