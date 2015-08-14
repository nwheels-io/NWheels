using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Core
{
    public abstract class UserAccountEntity : IUserAccountEntity
    {
        public void ChangePassword(string newPassword)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityPartClaimsContainer

        public string[] AssociatedRoles { get; set; }
        public string[] AssociatedPermissions { get; set; }
        public string[] AssociatedDataRules { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IUserAccountEntity

        public string LoginName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? EmailVerifiedAtUtc { get; set; }
        public ICollection<IPasswordEntity> Passwords { get; protected set; }
        public DateTime? LastLoginAtUtc { get; set; }
        public int FailedLoginCount { get; set; }
        public bool IsLockedOut { get; set; }

        #endregion
    }
}
