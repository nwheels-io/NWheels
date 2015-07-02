using System.Security;
using System.Security.Principal;

namespace NWheels.Domains.Security.Core
{
    public interface IAuthenticationProvider
    {
        IPrincipal Authenticate(string loginName, SecureString password);
    }
}
