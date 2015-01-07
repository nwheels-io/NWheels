using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Crm
{
    public interface ICustomerRelationshipTypeEntity
    {
        string Name { get; set; }
        bool IsSymmetric { get; set; }
        bool IsTransitive { get; set; }
    }
}
