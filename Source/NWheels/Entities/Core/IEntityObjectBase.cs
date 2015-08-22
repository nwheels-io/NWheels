using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;

namespace NWheels.Entities.Core
{
    public interface IEntityObjectBase : IObject
    {
        EntityState State { get; }
    }
}
