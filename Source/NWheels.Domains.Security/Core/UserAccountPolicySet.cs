using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Core
{
    public class UserAccountPolicySet
    {
        public UserAccountPolicy GetPolicy(UserAccountEntity userAccount)
        {
            return new UserAccountPolicy(
                failedLoginAttemptsBeforeLockOut: 3,
                passwordExpiryDays: 90,
                passwordMinLength: 4,
                passwordMaxLength: 20,
                passwordMinAlphaChars: 1,
                passwordMinDigitChars: 1,
                passwordMinSpecialChars: 1,
                duplicateCheckLastPasswordCount: 4,
                passwordHistoryRetention: TimeSpan.FromDays(1095));
        }
    }
}
