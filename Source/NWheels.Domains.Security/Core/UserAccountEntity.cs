using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;
using NWheels.Domains.Security.Impl;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.Domains.Security.Core
{
    public abstract class UserAccountEntity : IUserAccountEntity, IActiveRecord
    {
        public void SetPassword(SecureString passwordString)
        {
            var policy = PolicySet.GetPolicy(this);

            DeactivateCurrentPassword();

            var password = Framework.New<IPasswordEntity>();
                
            password.User = this;
            password.Hash = CryptoProvider.CalculateHash(passwordString);
            password.ExpiresAtUtc = Framework.UtcNow.AddDays(policy.PasswordExpiryDays);
            password.As<IActiveRecord>().Save();

            this.Passwords.Add(password);
            this.Save();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetTemporaryPassword()
        {
            var policy = PolicySet.GetPolicy(this);

            DeactivateCurrentPassword();

            var password = Framework.New<IPasswordEntity>();
            var passwordLength = new Random().Next(policy.PasswordMinLength, policy.PasswordMaxLength);
            var clearText = Convert.ToBase64String(Framework.NewGuid().ToByteArray()).Substring(0, passwordLength);

            password.User = this;
            password.Hash = CryptoProvider.CalculateHash(SecureStringUtility.ClearToSecure(clearText));
            password.ExpiresAtUtc = Framework.UtcNow.AddDays(policy.TemporaryPasswordExpiryDays);
            password.MustChange = true;

            password.As<IActiveRecord>().Save();

            this.Passwords.Add(password);
            this.Save();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountPrincipal Authenticate(SecureString password)
        {
            var policy = PolicySet.GetPolicy(this);

            ValidateNotLockedOut();
            var activePassword = ValidatePasswordExpiry();
            ValidatePasswordMatch(password, activePassword, policy);
            var principal = CreatePrincipal();

            Logger.UserAuthenticated(LoginName);
            
            return principal;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountPrincipal CreatePrincipal()
        {
            var claims = ClaimFactory.CreateClaimsFromContainerEntity(this);
            var identity = new UserAccountIdentity(this, claims);
            var principal = new UserAccountPrincipal(identity);

            return principal;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IActiveRecord

        public abstract void Save();
        public abstract void Delete();

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Dependencies

        protected IFramework Framework { get; set; }
        protected ISecurityDomainLogger Logger { get; set; }
        protected ICryptoProvider CryptoProvider { get; set; }
        protected ClaimFactory ClaimFactory { get; set; }
        protected UserAccountPolicySet PolicySet { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityPartClaimsContainer

        public abstract string[] AssociatedRoles { get; set; }
        public abstract string[] AssociatedPermissions { get; set; }
        public abstract string[] AssociatedDataRules { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IUserAccountEntity

        public abstract string LoginName { get; set; }
        public abstract string FullName { get; set; }
        public abstract string EmailAddress { get; set; }
        public abstract DateTime? EmailVerifiedAtUtc { get; set; }
        public abstract ICollection<IPasswordEntity> Passwords { get; protected set; }
        public abstract DateTime? LastLoginAtUtc { get; set; }
        public abstract int FailedLoginCount { get; set; }
        public abstract bool IsLockedOut { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DeactivateCurrentPassword()
        {
            using ( var context = Framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                var now = Framework.UtcNow;

                foreach ( var oldPassword in this.Passwords )
                {
                    if ( !oldPassword.IsExpired(now) )
                    {
                        oldPassword.ExpiresAtUtc = now;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateNotLockedOut()
        {
            if ( IsLockedOut )
            {
                Logger.FailedLoginAttempt(LoginFault.AccountLockedOut, LoginName);
                throw new DomainFaultException<LoginFault>(LoginFault.AccountLockedOut);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IPasswordEntity ValidatePasswordExpiry()
        {
            var activePassword = Passwords.FirstOrDefault(p => !p.IsExpired(Framework.UtcNow));

            if ( activePassword == null )
            {
                Logger.FailedLoginAttempt(LoginFault.PasswordExpired, LoginName);
                throw new DomainFaultException<LoginFault>(LoginFault.PasswordExpired);
            }

            return activePassword;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void ValidatePasswordMatch(SecureString password, IPasswordEntity activePassword, UserAccountPolicy policy)
        {
            if ( !CryptoProvider.MatchHash(activePassword.Hash, password) )
            {
                Logger.FailedLoginAttempt(LoginFault.LoginIncorrect, LoginName);

                if ( ++FailedLoginCount >= policy.FailedLoginAttemptsBeforeLockOut )
                {
                    LockOut();
                }

                throw new DomainFaultException<LoginFault>(LoginFault.LoginIncorrect);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LockOut()
        {
            Logger.UserAccountLockedOut(LoginName);
            IsLockedOut = true;
        }
    }
}
