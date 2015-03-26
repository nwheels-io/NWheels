using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Modules.Auth
{
    [EntityContract]
    [MustMixIn(typeof(IEntityPartUserRoleId<>))]
    public interface IUserRoleEntity
    {
        string Name { get; set; }
    }
}
