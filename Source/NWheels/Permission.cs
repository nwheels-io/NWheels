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
    public static class Permission
    {
        public static void DemandClaims(params object[] claimEnumValues)
        {
            new EnumClaimsPermission(claimEnumValues).Demand();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void MustAuthenticate()
        {
            new UserIdentityClaimPermission().Demand();
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

        public class MustAuthenticateAttribute : CodeAccessSecurityAttribute
        {
            public MustAuthenticateAttribute(SecurityAction action)
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
    }
}
