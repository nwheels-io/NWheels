using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Core.Hosting
{
    internal enum NodeTrigger
    {
        Load,
        LoadSuccess,
        LoadFailure,
        Activate,
        ActivateSuccess,
        ActivateFailure,
        Deactivate,
        DeactivateDone,
        Unload,
        UnloadDone
    }
}
