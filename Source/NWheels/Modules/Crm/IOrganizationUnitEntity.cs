using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Crm
{
    public interface IOrganizationUnitEntity
    {
        string Name { get; set; }
        ICustomerEntity UnitContact { get; set; }
        IOrganizationEntity Organization { get; set; }
    }
}
