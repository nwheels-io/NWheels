using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Core
{
    public class UserAccountPolicy
    {
        public UserAccountPolicy(
            int loginMinLength,
            int loginMaxLength,
            int failedLoginAttemptsBeforeLockOut, 
            int passwordExpiryDays, 
            int temporaryPasswordExpiryDays,
            int passwordMinLength, 
            int passwordMaxLength, 
            int passwordMinAlphaChars, 
            int passwordMinDigitChars, 
            int passwordMinSpecialChars, 
            int duplicateCheckLastPasswordCount, 
            TimeSpan passwordHistoryRetention,
            TimeSpan emailVerificationLinkExpiry)
        {
            LoginMinLength = loginMinLength;
            LoginMaxLength = loginMaxLength;
            FailedLoginAttemptsBeforeLockOut = failedLoginAttemptsBeforeLockOut;
            PasswordExpiryDays = passwordExpiryDays;
            TemporaryPasswordExpiryDays = temporaryPasswordExpiryDays;
            PasswordMinLength = passwordMinLength;
            PasswordMaxLength = passwordMaxLength;
            PasswordMinAlphaChars = passwordMinAlphaChars;
            PasswordMinDigitChars = passwordMinDigitChars;
            PasswordMinSpecialChars = passwordMinSpecialChars;
            DuplicateCheckLastPasswordCount = duplicateCheckLastPasswordCount;
            PasswordHistoryRetention = passwordHistoryRetention;
            EmailVerificationLinkExpiry = emailVerificationLinkExpiry;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int LoginMinLength { get; private set; }
        public int LoginMaxLength { get; private set; }
        public int FailedLoginAttemptsBeforeLockOut { get; private set; }
        public int PasswordExpiryDays { get; private set; }
        public int TemporaryPasswordExpiryDays { get; private set; }
        public int PasswordMinLength { get; private set; }
        public int PasswordMaxLength { get; private set; }
        public int PasswordMinAlphaChars { get; private set; }
        public int PasswordMinDigitChars { get; private set; }
        public int PasswordMinSpecialChars { get; private set; }
        public int DuplicateCheckLastPasswordCount { get; private set; }
        public TimeSpan PasswordHistoryRetention { get; private set; }
        public TimeSpan EmailVerificationLinkExpiry { get; private set; }
    }
}
