using System;
using System.Collections.Generic;
using System.Text;
using NWheels;

namespace ElectricityBilling.Domain.Accounts
{
    public abstract class UserCredentialsValueObject
    {
        protected UserCredentialsValueObject(DateTime validSinceUtc)
        {
            this.ValidSinceUtc = validSinceUtc;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.Semantics.Utc]
        public DateTime ValidSinceUtc { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EmailPasswordCredentials : UserCredentialsValueObject
    {
        public EmailPasswordCredentials(DateTime validSinceUtc, string loginEmail, string password) 
            : base(validSinceUtc)
        {
            LoginEmail = loginEmail;
            Password = password;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.Required]
        [MemberContract.Semantics.EmailAddress]
        public string LoginEmail { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.Required]
        [MemberContract.Semantics.Password]
        public string Password { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SingleSignOnCredentials : UserCredentialsValueObject
    {
        public SingleSignOnCredentials(DateTime validSinceUtc) 
            : base(validSinceUtc)
        {
        }
    }
}
