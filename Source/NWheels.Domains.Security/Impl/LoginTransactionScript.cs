using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using NWheels.Authorization;
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

        public IIdentityInfo Execute(string loginName, SecureString password)
        {
            var principal = _authenticationProvider.Authenticate(loginName, password);

            using ( var data = _framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                var userAccount = principal.Identity.GetUserAccount();
                userAccount.LastLoginAtUtc = _framework.UtcNow;
                data.AllUsers.Update(userAccount);
                data.CommitChanges();
            }

            Thread.CurrentPrincipal = principal;
            return principal.Identity;
        }
    }
}
