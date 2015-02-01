using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Tools.LogViewer
{
    public enum LogNodeKind
    {
        ThreadSuccess,
        ThreadWarning,
        ThreadFailure,
        ActivitySuccess,
        ActivityWarning,
        ActivityFailure,
        LogDebug,
        LogVerbose,
        LogInfo,
        LogWarning,
        LogError,
        LogCritical
    }
}
