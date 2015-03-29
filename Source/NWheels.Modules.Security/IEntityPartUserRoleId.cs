using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Modules.Security
{
    [EntityPartContract]
    public interface IEntityPartUserRoleId<TRoleId>
    {
        [PropertyContract(IsRequired = true)]
        TRoleId RoleId { get; set; }
    }
}
