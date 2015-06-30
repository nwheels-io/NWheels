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
    public interface IUserRoleEntity<TRole> : IEntityPartUniqueDisplayName
    {
        [PropertyContract.Required]
        TRole Role { get; set; }
    }
}
