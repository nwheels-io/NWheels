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

        public UserAccountPrincipal Authenticate(string loginName, SecureString password)
        {
            using ( var data = _framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                var user = data.AllUsers.FirstOrDefault(u => u.LoginName == loginName);

                if ( user == null )
                {
                    _logger.UserNotFound(loginName);
                    throw new DomainFaultException<LoginFault>(LoginFault.LoginIncorrect);
                }

                var principal = user.As<UserAccountEntity>().Authenticate(password);
                
                data.AllUsers.Update(user);
                data.CommitChanges();
                
                return principal;
            }
        }
    }
}
