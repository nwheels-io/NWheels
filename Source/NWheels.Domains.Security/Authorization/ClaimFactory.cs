using System;
using System.Security.Claims;

namespace NWheels.Domains.Security.Authorization
{
    public static class ClaimFactory
    {
        public const string ClaimTypeRole = "ROLE";
        public const string ClaimTypeActionPermission = "ACTION";
        public const string ClaimTypeDataPermission = "DATA";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Claim FromClaimString(string claimString)
        {
            var colonIndex = claimString.IndexOf(":");
            
            if ( colonIndex < 1 || colonIndex > claimString.Length - 2 )
            {
                throw new FormatException();
            }

            var claimType = claimString.Substring(0, colonIndex);
            var claimValue = claimString.Substring(colonIndex + 1);

            if ( claimType != ClaimTypeRole && claimType != ClaimTypeActionPermission && claimType != ClaimTypeDataPermission )
            {
                throw new FormatException();
            }

            return new Claim(claimType, claimValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToClaimString(this Claim claim)
        {
            return string.Format("{0}:{1}", claim.Type, claim.Value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Claim UserRole(IUserRoleEntity role)
        {
            return new Claim(ClaimTypeRole, role.SystemName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Claim ActionPermission(IUserActionPermissionEntity permission)
        {
            return new Claim(ClaimTypeActionPermission, permission.SystemName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Claim DataPermission(IUserDataPermissionEntity permission)
        {
            return new Claim(ClaimTypeDataPermission, permission.SystemName);
        }
    }
}
