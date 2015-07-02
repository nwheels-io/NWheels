using System.Security;
using System.Security.Permissions;
using NWheels.Authorization.Claims;

namespace NWheels.Authorization
{
    public abstract class EnumClaimCodeAccessAttribute : CodeAccessSecurityAttribute
    {
        private readonly object[] _requiredClaimEnumValues;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected EnumClaimCodeAccessAttribute(SecurityAction action, params object[] requiredClaimEnumValues)
            : base(action)
        {
            _requiredClaimEnumValues = requiredClaimEnumValues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of SecurityAttribute

        public override IPermission CreatePermission()
        {
            return new EnumClaimsPermission(_requiredClaimEnumValues);
        }

        #endregion
    }
}
