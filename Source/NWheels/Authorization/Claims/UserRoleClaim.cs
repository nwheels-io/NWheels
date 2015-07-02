using System.Collections.Generic;
using System.Security.Claims;

namespace NWheels.Authorization.Claims
{
    public class UserRoleClaim : EnumClaimBase, IImplyMoreClaims
    {
        public static readonly string UserRoleClaimTypeString = "UserRole";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserRoleClaim(object userRoleEnumValue)
            : base(UserRoleClaimTypeString, GetEnumValueString(userRoleEnumValue))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEnumerable<Claim> GetImpliedClaims()
        {
            return new Claim[0];
        }
    }
}
