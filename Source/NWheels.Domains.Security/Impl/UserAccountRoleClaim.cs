using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Claims;

namespace NWheels.Domains.Security.Impl
{
    public class UserAccountRoleClaim : UserRoleClaim
    {
        private readonly ClaimFactory _claimFactory;
        private readonly IUserRoleEntity _userRole;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountRoleClaim(ClaimFactory claimFactory, IUserRoleEntity userRole)
            : base(ClaimFactory.ParseClaimEnumValue(userRole.ClaimValueType, userRole.ClaimValue))
        {
            _claimFactory = claimFactory;
            _userRole = userRole;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<Claim> GetImpliedClaims()
        {
            return _claimFactory.CreateClaimsFromContainerEntity(_userRole);
        }
    }
}
