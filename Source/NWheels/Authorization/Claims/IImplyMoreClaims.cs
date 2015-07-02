using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization.Claims
{
    /// <summary>
    /// Defines a claim, which implies a set of additional claims.
    /// </summary>
    public interface IImplyMoreClaims
    {
        /// <summary>
        /// For example: a user role imply default set of permissions and entity access rules, which by default, apply to members of the role.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Claim> GetImpliedClaims();
    }
}
