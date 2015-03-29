using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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
        [PropertyContract.Required, PropertyContract.Unique, PropertyContract.Semantic.DataType(typeof(LoginNameDataType))]
        string LoginName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.Validation.Length(min: 2, max: 100)]
        string FullName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        ICollection<IPasswordEntity> Passwords { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime? LastLoginAt { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Validation.MinValue(0)]
        int FailedLoginCount { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        bool IsLockedOut { get; set; }
    }
}
