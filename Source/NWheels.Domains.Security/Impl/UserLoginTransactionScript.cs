using System;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Domains.Security.Core;
using NWheels.Exceptions;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Domains.Security.Impl
{
    public class UserLoginTransactionScript : ITransactionScript
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

        public void Execute(
            [PropertyContract.Semantic.LoginName] 
            string loginName,
            [PropertyContract.Semantic.Password] 
            string password)
        {
            var principal = _authenticationProvider.Authenticate(loginName, SecureStringUtility.ClearToSecure(password));
            _sessionManager.AuthorieSession(principal);
        }
    }
}
