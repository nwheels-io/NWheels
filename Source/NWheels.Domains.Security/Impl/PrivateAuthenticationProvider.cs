using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.Security.Core;
using NWheels.Exceptions;
using NWheels.Extensions;

namespace NWheels.Domains.Security.Impl
{
    public class PrivateAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IFramework _framework;
        private readonly ISecurityDomainLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PrivateAuthenticationProvider(IFramework framework, ISecurityDomainLogger logger)
        {
            _framework = framework;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountPrincipal Authenticate(
            IQueryable<IUserAccountEntity> userAccounts, 
            string loginName, 
            SecureString password, 
            out IUserAccountEntity userAccount)
        {
            var lowercaseLoginName = loginName.ToLower();
            userAccount = userAccounts.FirstOrDefault(u => u.LoginName.ToLower() == lowercaseLoginName);

            if ( userAccount == null )
            {
                _logger.UserNotFound(loginName);
                throw new DomainFaultException<LoginFault>(LoginFault.LoginIncorrect);
            }

            var principal = userAccount.As<UserAccountEntity>().Authenticate(password);
            return principal;
        }
    }
}
