using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NWheels.Samples.MyHRApp.Authorization
{
    public static class HRClaims
    {
        public const string UserRoleAdministrator = "MyHR.UserRole.Administrator";
        public const string UserRoleHRManager = "MyHR.UserRole.HRManager";
        public const string AclAdministrator = "MyHR.ACL.Administrator";
    }
}