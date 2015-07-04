using System.Security.Claims;

namespace NWheels.Authorization.Claims
{
    public class OperationPermissionClaim : Claim
    {
        public static readonly string OperationPermissionClaimTypeString = "OperationPermission";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OperationPermissionClaim(string claimValue)
            : base(OperationPermissionClaimTypeString, claimValue)
        {
        }
    }
}
