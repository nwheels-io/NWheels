using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Auth
{
    [EntityContract]
    public interface IPasswordEntity
    {
        [Required]
        IUserAccountEntity User { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Required]
        byte[] Hash { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime? Expiration { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool MustChange { get; set; }
    }
}
