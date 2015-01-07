using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Auth
{
    public interface IEntityPartAudit
    {
        DateTime CreatedAt { get; set; }
        IUserAccountEntity CreatedBy { get; set; }
        DateTime ModifiedAt { get; set; }
        IUserAccountEntity ModifiedBy { get; set; }
    }
}
