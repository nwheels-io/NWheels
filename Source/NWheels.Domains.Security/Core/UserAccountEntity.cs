using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
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

            var password = Framework.NewDomainObject<IPasswordEntityPart>();
                
            password.Hash = CryptoProvider.CalculateHash(passwordString);
            password.ExpiresAtUtc = Framework.UtcNow.AddDays(policy.PasswordExpiryDays);

            this.Passwords.Add(password);
            this.Save();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SetTemporaryPassword()
        {
            var policy = PolicySet.GetPolicy(this);

            DeactivateCurrentPassword();

            var password = Framework.NewDomainObject<IPasswordEntityPart>();
            var passwordLength = new Random().Next(policy.PasswordMinLength, policy.PasswordMaxLength);
            var clearText = GenerateTemporaryPassword(passwordLength);

            password.Hash = CryptoProvider.CalculateHash(SecureStringUtility.ClearToSecure(clearText));
            password.ExpiresAtUtc = Framework.UtcNow.AddDays(policy.TemporaryPasswordExpiryDays);
            password.MustChange = true;

            this.Passwords.Add(password);
            this.Save();

            return clearText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AutoGenerateLoginName(string baseText)
        {
            this.LoginName = CreateLoginName(baseText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string CreateLoginName(string baseText)
        {
            var policy = PolicySet.GetPolicy(this);
            var builder = new StringBuilder();

            for ( int i = 0 ; i < baseText.Length && i < Math.Max(policy.LoginMaxLength - 3, 3) ; i++ )
            {
                if ( char.IsLetterOrDigit(baseText[i]) )
                {
                    builder.Append(baseText[i]);
                }
            }

            var random = new Random();

            while ( builder.Length < policy.LoginMinLength + (policy.LoginMaxLength - policy.LoginMinLength) / 2 )
            {
                builder.Append(random.Next(1, 100));
            }

            var loginName = builder.ToString();

            if ( IsLoginNameAvailable(Framework, loginName) )
            {
                return loginName;
            }
                
            for ( int suffix = 1 ; suffix < 1000 ; suffix++ )
            {
                if ( IsLoginNameAvailable(Framework, loginName + suffix) )
                {
                    return loginName + suffix;
                }
            }

            throw new SecurityException("Unable to generate unique login name.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountPrincipal Authenticate(SecureString password)
        {
            var policy = PolicySet.GetPolicy(this);

            ValidateNotLockedOut();
            var activePassword = ValidatePasswordExpiry();
            ValidatePasswordMatch(password, activePassword, policy);
            ValidatePasswordMustChange(activePassword);

            var principal = CreatePrincipal();

            Logger.UserAuthenticated(LoginName);
            
            return principal;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountPrincipal CreatePrincipal()
        {
            var claims = ClaimFactory.CreateClaimsFromContainerEntity(this).ToArray();
            var accessControlList = AccessControlCache.GetAccessControlList(claims);
            var identity = new UserAccountIdentity(this, claims, accessControlList);
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
        protected AccessControlListCache AccessControlCache { get; set; }
        protected UserAccountPolicySet PolicySet { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityPartClaimsContainer

        public abstract ICollection<IUserRoleEntity> AssociatedRoles { get; set; }
        public abstract ICollection<IOperationPermissionEntity> AssociatedPermissions { get; set; }
        public abstract ICollection<IEntityAccessRuleEntity> AssociatedEntityAccessRules { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IUserAccountEntity

        public abstract string LoginName { get; set; }
        public abstract string FullName { get; set; }
        public abstract string EmailAddress { get; set; }
        public abstract DateTime? EmailVerifiedAtUtc { get; set; }
        public abstract ICollection<IPasswordEntityPart> Passwords { get; protected set; }
        public abstract DateTime CreatedAtUtc { get; set; }
        public abstract DateTime? LastLoginAtUtc { get; set; }
        public abstract int FailedLoginCount { get; set; }
        public abstract bool IsLockedOut { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.TriggerOnNew]
        protected virtual void EntityTriggerAfterNew()
        {
            this.CreatedAtUtc = Framework.UtcNow;
        }

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
                        oldPassword.ExpiresAtUtc = now.Date;
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

        private IPasswordEntityPart ValidatePasswordExpiry()
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
        
        private void ValidatePasswordMatch(SecureString password, IPasswordEntityPart activePassword, UserAccountPolicy policy)
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

        private void ValidatePasswordMustChange(IPasswordEntityPart activePassword)
        {
            if ( activePassword.MustChange )
            {
                throw new DomainFaultException<LoginFault>(LoginFault.PasswordExpired);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LockOut()
        {
            Logger.UserAccountLockedOut(LoginName);
            IsLockedOut = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GenerateTemporaryPassword(int passwordLength)
        {
            var passwordBytes = Framework.NewGuid().ToByteArray();
            return Convert.ToBase64String(passwordBytes).Substring(0, passwordLength).Replace('/', 'A').Replace('+', 'B');
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsLoginNameAvailable(IFramework framework, string loginName)
        {
            using ( var context = framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                return !context.AllUsers.Any(u => u.LoginName == loginName);
            }
        }
    }
}
