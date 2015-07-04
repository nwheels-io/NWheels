using System.Collections.Generic;
using System.Security.Claims;

namespace NWheels.Authorization.Claims
{
    public class UserRoleClaim : Claim, IImplyMoreClaims
    {
        public static readonly string UserRoleClaimTypeString = "UserRole";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserRoleClaim(string claimValue)
            : base(UserRoleClaimTypeString, claimValue)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEnumerable<Claim> GetImpliedClaims()
        {
            return new Claim[0];
        }
    }
}
