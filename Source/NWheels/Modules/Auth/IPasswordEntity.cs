using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Auth
{
    public interface IPasswordEntity
    {
        IUserAccountEntity User { get; set; }
        byte[] Hash { get; set; }
        DateTime Expiration { get; set; }
        bool MustChange { get; set; }
    }
}
