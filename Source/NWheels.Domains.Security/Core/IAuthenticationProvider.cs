using System.Linq;
using System.Security;
using System.Security.Principal;

namespace NWheels.Domains.Security.Core
{
    public interface IAuthenticationProvider
    {
        UserAccountPrincipal Authenticate(IQueryable<IUserAccountEntity> userAccounts, string loginName, SecureString password, out IUserAccountEntity userAccount);
    }
}
