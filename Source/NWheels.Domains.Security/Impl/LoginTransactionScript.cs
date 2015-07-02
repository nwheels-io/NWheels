using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using NWheels.Domains.Security.Core;
using NWheels.Exceptions;

namespace NWheels.Domains.Security.Impl
{
    public class LoginTransactionScript
    {
        private readonly IFramework _framework;
        private readonly IAuthenticationProvider _authenticationProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LoginTransactionScript(IFramework framework, IAuthenticationProvider authenticationProvider)
        {
            _framework = framework;
            _authenticationProvider = authenticationProvider;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute(string loginName, SecureString password)
        {
            Thread.CurrentPrincipal = _authenticationProvider.Authenticate(loginName, password);
        }
    }
}
