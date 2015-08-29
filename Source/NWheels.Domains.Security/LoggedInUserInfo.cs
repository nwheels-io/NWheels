using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security
{
    public class LoggedInUserInfo
    {

        public string FullName { get; private set; }
        public string AccountType { get; private set; }
        public string[] UserRoles { get; private set; }
        public string[] AllClaims { get; private set; }
        public DateTime? LastLoginAtUtc { get; private set; }
    }
}
