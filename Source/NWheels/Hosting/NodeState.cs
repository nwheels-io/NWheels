using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Hosting
{
    public enum NodeState
    {
        Down,
        Loading,
        Standby,
        Activating,
        Active,
        Deactivating,
        Unloading
    }
}
