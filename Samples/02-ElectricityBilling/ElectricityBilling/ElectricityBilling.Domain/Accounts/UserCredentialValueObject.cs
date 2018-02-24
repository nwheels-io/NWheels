using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using NWheels;
using NWheels.Ddd;
using NWheels.Microservices;

namespace ElectricityBilling.Domain.Accounts
{
    public abstract class UserCredentialValueObject
    {
        [MemberContract.Semantics.Utc]
        private readonly DateTime _validSinceUtc;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UserCredentialValueObject(DateTime validSinceUtc)
        {
            validSinceUtc = ValidSinceUtc;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime ValidSinceUtc => _validSinceUtc;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EmailPasswordCredential : UserCredentialValueObject
    {
        [MemberContract.Required]
        [MemberContract.Validation.Unique]
        [MemberContract.Semantics.EmailAddress]
        private readonly string _loginEmail;

        private readonly ImmutableList<PasswordValueObject> _passwords = ImmutableList<PasswordValueObject>.Empty;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EmailPasswordCredential(
            Injector<IPasswordCredentialPolicy> injector, 
            DateTime validSinceUtc, string loginEmail, string passwordClearText) 
            : base(validSinceUtc)
        {
            _loginEmail = loginEmail;
            _passwords = _passwords.Add(new PasswordValueObject(injector, passwordClearText));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string LoginEmail => _loginEmail;
        public ImmutableList<PasswordValueObject> Passwords => _passwords;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct PasswordValueObject
    {
        [MemberContract.Required]
        [MemberContract.Semantics.PasswordCipher]
        private readonly ImmutableArray<byte> _hash;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PasswordValueObject(
            Injector<IPasswordCredentialPolicy> injector, 
            string clearText)
        {
            injector.Inject(out IPasswordCredentialPolicy policy);
            _hash = policy.CalculateHash(clearText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImmutableArray<byte> Hash => _hash;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SingleSignOnCredential : UserCredentialValueObject
    {
        public SingleSignOnCredential(DateTime validSinceUtc) 
            : base(validSinceUtc)
        {
        }
    }
}
