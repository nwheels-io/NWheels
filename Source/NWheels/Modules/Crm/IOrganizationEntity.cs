using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Crm
{
    public interface IOrganizationEntity
    {
        string Name { get; set; }
        ICustomerEntity OrganizationContact { get; set; }
        IList<IOrganizationUnitEntity> OrganizationUnits { get; set; }
    }
}
