using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Modules.Security.Domain;

namespace NWheels.Modules.Security
{
    [EntityContract]
    public interface IUserAccountEntity
    {
        [PropertyContract(typeof(LoginNameDataType), IsRequired = true)]
        string LoginName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(typeof(PasswordDataType), IsRequired = true)]
        string FullName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(SemanticType.EmailAddress, IsRequired = true)]
        string EmailAddress { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IsEmailVerified { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        ICollection<IPasswordEntity> Passwords { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        ICollection<IUserRoleEntity> Roles { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime? LastLoginAt { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        int FailedLoginCount { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        bool IsLockedOut { get; set; }
    }
}
