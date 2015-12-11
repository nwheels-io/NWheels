using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Domains.Security.Core;
using NWheels.Entities;
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
            IApplicationDataRepository authenticationContext;
            IQueryable<IUserAccountEntity> userAccountQuery;
            OpenAuthenticationContext(out authenticationContext, out userAccountQuery);

            using ( authenticationContext )
            {
                IUserAccountEntity userAccount = null;

                try
                {
                    _authenticationProvider.Authenticate(userAccountQuery, loginName, SecureStringUtility.ClearToSecure(oldPassword), out userAccount);
                }
                catch ( DomainFaultException<LoginFault> error )
                {
                    if ( error.FaultCode != LoginFault.PasswordExpired )
                    {
                        throw;
                    }
                }

                userAccount.As<UserAccountEntity>().SetPassword(SecureStringUtility.ClearToSecure(newPassword));
                UpdateUserAccount(authenticationContext, userAccount);

                authenticationContext.CommitChanges();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OpenAuthenticationContext(out IApplicationDataRepository context, out IQueryable<IUserAccountEntity> userAccounts)
        {
            var userAccountsContext = _framework.NewUnitOfWork<IUserAccountDataRepository>();

            context = userAccountsContext;
            userAccounts = userAccountsContext.AllUsers;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void UpdateUserAccount(IApplicationDataRepository context, IUserAccountEntity userAccount)
        {
            ((IUserAccountDataRepository)context).AllUsers.Update(userAccount);
        }
    }
}
