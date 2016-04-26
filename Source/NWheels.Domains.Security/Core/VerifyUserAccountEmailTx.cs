using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;
using NWheels.Processing;

namespace NWheels.Domains.Security.Core
{
    public class VerifyUserAccountEmailTx : ITransactionScript
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public VerifyUserAccountEmailTx(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute(string linkId)
        {
            using (var context = _framework.NewUnitOfWork<IUserAccountDataRepository>())
            {
                var userAccount = context.AllUsers.AsQueryable().FirstOrDefault(u => u.EmailVerification.EmailVerificationLinkUniqueId == linkId);

                if (userAccount == null)
                {
                    throw new DomainFaultException<EmailVerificationFault>(EmailVerificationFault.InvalidVerificationLink);
                }

                userAccount.VerifyEmail(linkId);

                context.AllUsers.Update(userAccount);
                context.CommitChanges();
            }
        }
    }
}
