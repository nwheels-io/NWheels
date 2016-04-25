using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    public interface IEntitySelfAccessControl
    {
        void SetMemberAccessControl(IEntityMemberAccessControl access);
        bool? CanUpdateEntity { get; }
        bool? CanDeleteEntity { get; }
    }
}
