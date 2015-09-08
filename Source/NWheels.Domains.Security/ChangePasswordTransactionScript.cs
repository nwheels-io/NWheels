using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Domains.Security.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Domains.Security
{
    public class ChangePasswordTransactionScript : ITransactionScript
    {
        private readonly IFramework _framework;
        private readonly IAuthenticationProvider _authenticationProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChangePasswordTransactionScript(IFramework framework, IAuthenticationProvider authenticationProvider)
        {
            _framework = framework;
            _authenticationProvider = authenticationProvider;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute(
            [PropertyContract.Semantic.LoginName] 
            string loginName,
            [PropertyContract.Semantic.Password] 
            string oldPassword,
            [PropertyContract.Semantic.Password] 
            string newPassword)
        {
            using ( var context = _framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                IUserAccountEntity userAccount = null;

                try
                {
                    _authenticationProvider.Authenticate(loginName, SecureStringUtility.ClearToSecure(oldPassword), out userAccount);
                }
                catch ( DomainFaultException<LoginFault> error )
                {
                    if ( error.FaultCode != LoginFault.PasswordExpired )
                    {
                        throw;
                    }
                }

                userAccount.As<UserAccountEntity>().SetPassword(SecureStringUtility.ClearToSecure(newPassword));
                context.AllUsers.Update(userAccount);

                context.CommitChanges();
            }
        }
    }
}
