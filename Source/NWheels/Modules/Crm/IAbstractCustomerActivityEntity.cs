using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Auth;

namespace NWheels.Modules.Crm
{
    public interface IAbstractCustomerActivityEntity
    {
        ICustomerEntity Customer { get; set; }
        DateTime RecordedAt { get; set; }
        IUserAccountEntity StaffMember { get; set; }
    }
}
