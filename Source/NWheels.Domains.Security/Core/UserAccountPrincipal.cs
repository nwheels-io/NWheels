using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Core
{
    public class UserAccountPrincipal : ClaimsPrincipal
    {
        public UserAccountPrincipal(UserAccountIdentity identity)
            : base(identity)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new UserAccountIdentity Identity
        {
            get
            {
                return (UserAccountIdentity)base.Identity;
            }
        }
    }
}
