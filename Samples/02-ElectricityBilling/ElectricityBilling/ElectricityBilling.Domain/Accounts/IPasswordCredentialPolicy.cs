using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ElectricityBilling.Domain.Accounts
{
    public interface IPasswordCredentialPolicy
    {
        bool Validate(string clearText, IEnumerable<PasswordValueObject> oldPasswords);
        ImmutableArray<byte> CalculateHash(string clearText);
        TimeSpan ValidityPeriod { get; }
        int MinLength { get; }
        int MaxLength { get; }
        int MinLowercaseChars { get; }
        int MinUppercaseChars { get; }
        int MinNumericChars { get; }
        int MinSpecialChars { get; }
    }
}
