using System.Security;
using System.Security.Principal;

namespace NWheels.Domains.Security.Core
{
    public interface IAuthenticationProvider
    {
        UserAccountPrincipal Authenticate(string loginName, SecureString password, out IUserAccountEntity userAccount);
    }
}
