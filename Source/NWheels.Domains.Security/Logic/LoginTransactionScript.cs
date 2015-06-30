using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Exceptions;

namespace NWheels.Domains.Security.Logic
{
    public class LoginTransactionScript
    {
        private readonly IFramework _framework;
        private readonly ICryptoProvider _cryptoProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LoginTransactionScript(IFramework framework, ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IUserAccountEntity Execute(string loginName, string password)
        {
            using ( var data = _framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                var user = data.AllUsers.FirstOrDefault(u => u.LoginName == loginName);

                if ( user == null )
                {
                    throw new DomainFaultException<LoginFault>(LoginFault.LoginIncorrect);
                }

                if ( user.IsLockedOut )
                {
                    throw new DomainFaultException<LoginFault>(LoginFault.AccountLockedOut);
                }

                if ( !user.Passwords.Any(p => !p.IsExpired(_framework.UtcNow) && _cryptoProvider.MatchHash(p.Hash, password)) )
                {
                    throw new DomainFaultException<LoginFault>(LoginFault.LoginIncorrect);
                }

                user.LastLoginAtUtc = _framework.UtcNow;
                data.AllUsers.Update(user);
                data.CommitChanges();

                //Thread.CurrentPrincipal = 

                return user;
            }
        }
    }
}
