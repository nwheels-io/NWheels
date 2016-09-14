using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
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
    public class ResetPasswordTransactionScript : ITransactionScript
    {
        private readonly IFramework _framework;
        private readonly ISecurityDomainLogger _logger;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ResetPasswordTransactionScript(IFramework framework, ISecurityDomainLogger logger, ISessionManager sessionManager)
        {
            _framework = framework;
            _logger = logger;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SecurityCheck.AllowAnonymous]
        public virtual void Execute([PropertyContract.Semantic.LoginName] string loginName)
        {
            using (_sessionManager.JoinGlobalSystem())
            {
                IApplicationDataRepository authenticationContext;
                IQueryable<IUserAccountEntity> userAccountQuery;
                OpenAuthenticationContext(out authenticationContext, out userAccountQuery);

                using (authenticationContext)
                {
                    var lowercaseLoginName = loginName.ToLower();
                    var userAccount = (UserAccountEntity)userAccountQuery.FirstOrDefault(u => u.LoginName.ToLower() == lowercaseLoginName);

                    if (userAccount == null)
                    {
                        _logger.ResetPasswordRequestDeclined(loginName);
                        return;
                    }


                    var temporaryPassword = userAccount.As<UserAccountEntity>().SetTemporaryPassword();
                    UpdateUserAccount(authenticationContext, userAccount);

                    userAccount.SendEmail(
                        UserEmailTemplateType.ResetPassword,
                        new {
                            UserAccount = userAccount,
                            TemporaryPassword = temporaryPassword
                        });

                    _logger.ResetPasswordRequestAccepted(loginName, EntityId.ValueOf(userAccount).ToString(), userAccount.EmailAddress);
                    authenticationContext.CommitChanges();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OpenAuthenticationContext(out IApplicationDataRepository context, out IQueryable<IUserAccountEntity> userAccounts)
        {
            var userAccountsContext = _framework.NewUnitOfWork<IUserAccountDataRepository>();

            context = userAccountsContext;
            userAccounts = userAccountsContext.AllUsers.AsQueryable();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void UpdateUserAccount(IApplicationDataRepository context, IUserAccountEntity userAccount)
        {
            ((IUserAccountDataRepository)context).AllUsers.Update(userAccount);
        }
    }
}
