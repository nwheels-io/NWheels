using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using NWheels.Authorization.Core;
using NWheels.Exceptions;
using NWheels.Domains.Security.Impl;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;
using NWheels.Utilities;

namespace NWheels.Domains.Security.Core
{
    public abstract class UserAccountEntity : IUserAccountEntity, IActiveRecord
    {
        public virtual void SetPassword(SecureString passwordString)
        {
            var policy = PolicySet.GetPolicy(this);

            DeactivateCurrentPassword();

            var password = Framework.NewDomainObject<IPasswordEntityPart>();
                
            password.Hash = CryptoProvider.CalculateHash(passwordString, salt: this.LoginName);
            password.ExpiresAtUtc = Framework.UtcNow.AddDays(policy.PasswordExpiryDays);

            this.Passwords.Add(password);
            this.IsLockedOut = false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string SetTemporaryPassword()
        {
            var policy = PolicySet.GetPolicy(this);

            DeactivateCurrentPassword();

            var random = new Random();
            var tempPasswordLength = random.Next(policy.PasswordMinLength, policy.PasswordMaxLength);
            var tempPasswordMinNonAlphaChars = policy.PasswordMinDigitChars + policy.PasswordMinSpecialChars;
            var tempPasswordClearText = Membership.GeneratePassword(tempPasswordLength, tempPasswordMinNonAlphaChars);

            var password = Framework.NewDomainObject<IPasswordEntityPart>();
            password.Hash = CryptoProvider.CalculateHash(SecureStringUtility.ClearToSecure(tempPasswordClearText), salt: this.LoginName);
            password.ExpiresAtUtc = Framework.UtcNow.AddHours(policy.TemporaryPasswordExpiryHours);

            this.Passwords.Add(password);
            this.IsLockedOut = false;

            Logger.UserTemporaryPasswordSet(LoginName, EntityId.ValueOf(this).ToString(), EmailAddress);
            return tempPasswordClearText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void SendEmail(object emailContentType, object data)
        {
            var email = new OutgoingEmailMessage(Framework, TemplateProvider);

            email.LoadTemplate(emailContentType, subjectAtFirstLine: true);
            email.TemplateData = data;

            SetUserEmailRecipients(email);
            ServiceBus.EnqueueMessage(email);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void SendEmailVerification(object emailContentType)
        {
            var policy = PolicySet.GetPolicy(this);
            var now = Framework.UtcNow;

            this.EmailVerification.EmailVerificationLinkSentAtUtc = now;
            this.EmailVerification.EmailVerificationLinkValidUntil = now.Add(policy.EmailVerificationLinkExpiry);
            this.EmailVerification.EmailVerificationLinkUniqueId = GenerateEmailVerificationLinkId();

            SendEmail(emailContentType, this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void VerifyEmail(string linkUniqueId)
        {
            var now = Framework.UtcNow;

            if (string.IsNullOrWhiteSpace(linkUniqueId) || linkUniqueId != this.EmailVerification.EmailVerificationLinkUniqueId)
            {
                throw new DomainFaultException<EmailVerificationFault>(EmailVerificationFault.InvalidVerificationLink);
            }

            if (now > this.EmailVerification.EmailVerificationLinkValidUntil.GetValueOrDefault(DateTime.MinValue))
            {
                throw new DomainFaultException<EmailVerificationFault>(EmailVerificationFault.InvalidVerificationLink);
            }

            if (!this.EmailVerification.EmailVerifiedAtUtc.HasValue)
            {
                this.EmailVerification.EmailVerifiedAtUtc = now;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void AutoGenerateLoginName(string baseText)
        {
            this.LoginName = CreateLoginName(baseText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string GenerateEmailVerificationLinkId()
        {
            return Framework.NewGuid().ToString("N");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void SetUserEmailRecipients(OutgoingEmailMessage email)
        {
            email.To.AddRecipient(this.FullName, this.EmailAddress);
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

        public virtual UserAccountPrincipal Authenticate(SecureString password, bool passwordExpired = false)
        {
            var policy = PolicySet.GetPolicy(this);

            ValidateNotLockedOut();
            
            var activePassword = (
                passwordExpired 
                ? FindMostRecentExpiredPassword() 
                : FindValidPassword());
            
            ValidatePasswordMatch(password, activePassword, policy);

            if (!passwordExpired)
            {
                ValidatePasswordMustChange(activePassword);
            }

            var principal = CreatePrincipal();
            
            Logger.UserAuthenticated(LoginName);
            return principal;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual UserAccountPrincipal CreatePrincipal()
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

        [EntityImplementation.DependencyProperty]
        protected IFramework Framework { get; set; }
        
        [EntityImplementation.DependencyProperty]
        protected ISecurityDomainLogger Logger { get; set; }
        
        [EntityImplementation.DependencyProperty]
        protected ICryptoProvider CryptoProvider { get; set; }
        
        [EntityImplementation.DependencyProperty]
        protected ClaimFactory ClaimFactory { get; set; }
        
        [EntityImplementation.DependencyProperty]
        protected AccessControlListCache AccessControlCache { get; set; }

        [EntityImplementation.DependencyProperty]
        protected IUserAccountPolicySet PolicySet { get; set; }

        [EntityImplementation.DependencyProperty]
        protected IContentTemplateProvider TemplateProvider { get; set; }

        [EntityImplementation.DependencyProperty]
        protected IServiceBus ServiceBus { get; set; }

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
        public abstract IEmailVerificationEntityPart EmailVerification { get; }
        public abstract ICollection<IPasswordEntityPart> Passwords { get; protected set; }
        public abstract DateTime CreatedAtUtc { get; set; }
        public abstract DateTime? LastLoginAtUtc { get; set; }
        public abstract int FailedLoginCount { get; set; }
        public abstract bool IsLockedOut { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsPasswordExpired
        {
            get
            {
                return EmailVerification.EmailVerifiedAtUtc.HasValue;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsEmailVerified
        {
            get
            {
                return EmailVerification.EmailVerifiedAtUtc.HasValue;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UserRolesText
        {
            get
            {
                return string.Join(", ", this.AssociatedRoles.Select(r => r.Name));
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.TriggerOnNew]
        protected virtual void EntityTriggerAfterNew()
        {
            this.CreatedAtUtc = Framework.UtcNow;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected bool HaveUserRole(IUserRoleEntity userRole)
        {
            return AssociatedRoles.Any(r => EntityId.ValueOf(r) == EntityId.ValueOf(userRole));
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
                Logger.AccountLockedOut(LoginName, EntityId.ValueOf(this).ToString(), EmailAddress);
                throw new DomainFaultException<LoginFault>(LoginFault.AccountLockedOut);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IPasswordEntityPart FindValidPassword()
        {
            var activePassword = Passwords.FirstOrDefault(p => !p.IsExpired(Framework.UtcNow));

            if ( activePassword == null )
            {
                Logger.UserLoginFailed(LoginName, LoginFault.PasswordExpired);
                throw new DomainFaultException<LoginFault>(LoginFault.PasswordExpired);
            }

            return activePassword;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IPasswordEntityPart FindMostRecentExpiredPassword()
        {
            var now = Framework.UtcNow;

            if (Passwords.Any(p => !p.IsExpired(now)))
            {
                throw new SecurityException("Cannot use expired password because a valid password exists.");
            }

            var expiredPassword = Passwords.OrderByDescending(p => p.ExpiresAtUtc).FirstOrDefault();

            if (expiredPassword == null)
            {
                throw new SecurityException("User account has no passwords.");
            }

            return expiredPassword;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void ValidatePasswordMatch(SecureString password, IPasswordEntityPart activePassword, UserAccountPolicy policy)
        {
            if ( !CryptoProvider.MatchHash(activePassword.Hash, password, LoginName) )
            {
                Logger.UserLoginFailed(LoginName, LoginFault.LoginIncorrect);

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
            Logger.AccountLockedOut(LoginName, EntityId.ValueOf(this).ToString(), EmailAddress);
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
                return !context.AllUsers.AsQueryable().Any(u => u.LoginName == loginName);
            }
        }
    }
}
