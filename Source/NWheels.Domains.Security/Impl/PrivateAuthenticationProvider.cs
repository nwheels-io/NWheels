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
            return InternalAuthenticate(userAccounts, loginName, password, passwordExpired: false, userAccount: out userAccount);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountPrincipal AuthenticateByExpiredPassword(
            IQueryable<IUserAccountEntity> userAccounts,
            string loginName,
            SecureString password,
            out IUserAccountEntity userAccount)
        {
            return InternalAuthenticate(userAccounts, loginName, password, passwordExpired: true, userAccount: out userAccount);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountPrincipal AuthenticateBySignInToken(
            IQueryable<IUserAccountEntity> userAccounts, 
            string token, 
            out IUserAccountEntity userAccount)
        {
            var now = _framework.UtcNow;
            userAccount = userAccounts.FirstOrDefault(u => u.SignInToken.Token == token && u.SignInToken.ExpiresAtUtc < now);

            if (userAccount == null)
            {
                _logger.UserNotFoundByToken(token);
                throw new DomainFaultException<LoginFault>(LoginFault.LoginIncorrect);
            }

            var principal = userAccount.As<UserAccountEntity>().AuthenticateBySignInToken(token);
            return principal;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private UserAccountPrincipal InternalAuthenticate(
            IQueryable<IUserAccountEntity> userAccounts, 
            string loginName, 
            SecureString password, 
            bool passwordExpired,
            out IUserAccountEntity userAccount)
        {
            var lowercaseLoginName = loginName.ToLower();
            userAccount = userAccounts.FirstOrDefault(u => u.LoginName.ToLower() == lowercaseLoginName);

            if ( userAccount == null )
            {
                _logger.UserNotFound(loginName);
                throw new DomainFaultException<LoginFault>(LoginFault.LoginIncorrect);
            }

            var principal = userAccount.As<UserAccountEntity>().Authenticate(password, passwordExpired);
            return principal;
        }
    }
}
