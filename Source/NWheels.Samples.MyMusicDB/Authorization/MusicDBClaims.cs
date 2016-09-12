using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NWheels.Samples.MyMusicDB.Authorization
{
    public static class MusicDBClaims
    {
        public const string UserRoleAdministrator = "MusicDB.UserRole.Administrator";
        public const string UserRoleOperator = "MusicDB.UserRole.Operator";
        public const string AclAdministrator = "MusicDB.ACL.Administrator";
        public const string AclOperator = "MusicDB.ACL.Operator";
    }
}
