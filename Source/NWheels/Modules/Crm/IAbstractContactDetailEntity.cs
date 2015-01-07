using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Crm
{
    public interface IAbstractContactDetailEntity : IEntityPartOrderBy
    {
        ICustomerEntity Customer { get; set; }
    }
}
