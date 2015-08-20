using System;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Domains.Security.Core;
using NWheels.Exceptions;

namespace NWheels.Domains.Security.Impl
{
    public class UserLoginTransactionScript
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ICoreSessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginTransactionScript(IAuthenticationProvider authenticationProvider, ICoreSessionManager sessionManager)
        {
            _authenticationProvider = authenticationProvider;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISession Execute(string loginName, SecureString password)
        {
            var principal = _authenticationProvider.Authenticate(loginName, password);
            return _sessionManager.OpenSession(principal, originatorEndpoint: null);
        }
    }
}
