using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Modules.Security
{
    [EntityContract]
    public interface IPasswordEntity
    {
        [PropertyContract.Required]
        IUserAccountEntity User { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.WriteOnly, PropertyContract.Security.Sensitive]
        string ClearText { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.SearchOnly, PropertyContract.Security.Sensitive]
        byte[] Hash { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime? Expiration { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool MustChange { get; set; }
    }
}
