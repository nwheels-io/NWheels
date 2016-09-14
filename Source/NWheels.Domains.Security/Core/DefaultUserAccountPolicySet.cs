using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Core
{
    public class DefaultUserAccountPolicySet : IUserAccountPolicySet
    {
        public virtual UserAccountPolicy GetPolicy(IUserAccountEntity userAccount)
        {
            return new UserAccountPolicy(
                loginMinLength: 5,
                loginMaxLength: 20,
                failedLoginAttemptsBeforeLockOut: 3,
                passwordExpiryDays: 90,
                temporaryPasswordExpiryHours: 48,
                passwordMinLength: 5,
                passwordMaxLength: 10,
                passwordMinAlphaChars: 1,
                passwordMinDigitChars: 1,
                passwordMinSpecialChars: 1,
                duplicateCheckLastPasswordCount: 4,
                passwordHistoryRetention: TimeSpan.FromDays(1095),
                emailVerificationLinkExpiry: TimeSpan.FromHours(72));
        }
    }
}
