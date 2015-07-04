using NWheels.Authorization.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels
{
    public static class SecurityCheck
    {
        public static void DemandAuthentication()
        {
            new UserIdentityClaimPermission().Demand();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void DemandUserRole(string userRole)
        {
            new ClaimsPermission(UserRoleClaim.UserRoleClaimTypeString, userRole).Demand();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void DemandPermission(string permission)
        {
            new ClaimsPermission(OperationPermissionClaim.OperationPermissionClaimTypeString, permission).Demand();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void DemandDataRule(string dataRule)
        {
            new ClaimsPermission(EntityAccessRuleClaim.EntityAccessRuleClaimTypeString, dataRule).Demand();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AllowAnonymousAttribute : CodeAccessSecurityAttribute
        {
            public AllowAnonymousAttribute(SecurityAction action)
                : base(action)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SecurityAttribute

            public override IPermission CreatePermission()
            {
                return new AnonymousAccessPermission();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AuthenticationAttribute : CodeAccessSecurityAttribute
        {
            public AuthenticationAttribute(SecurityAction action)
                : base(action)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SecurityAttribute

            public override IPermission CreatePermission()
            {
                return new UserIdentityClaimPermission();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class UserRoleAttribute : CodeAccessSecurityAttribute
        {
            public UserRoleAttribute(SecurityAction action)
                : base(action)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SecurityAttribute

            public override IPermission CreatePermission()
            {
                return new ClaimsPermission(UserRoleClaim.UserRoleClaimTypeString, this.UserRole);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string UserRole { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PermissionAttribute : CodeAccessSecurityAttribute
        {
            public PermissionAttribute(SecurityAction action)
                : base(action)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SecurityAttribute

            public override IPermission CreatePermission()
            {
                return new ClaimsPermission(OperationPermissionClaim.OperationPermissionClaimTypeString, this.Permission);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Permission { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DataRuleAttribute : CodeAccessSecurityAttribute
        {
            public DataRuleAttribute(SecurityAction action)
                : base(action)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SecurityAttribute

            public override IPermission CreatePermission()
            {
                return new ClaimsPermission(EntityAccessRuleClaim.EntityAccessRuleClaimTypeString, this.DataRule);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string DataRule { get; set; }
        }
    }
}
