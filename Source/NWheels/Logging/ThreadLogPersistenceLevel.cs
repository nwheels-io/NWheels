using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    public enum ThreadLogPersistenceLevel
    {
        None,
        StartupShutdown,
        StartupShutdownErrors,
        StartupShutdownErrorsWarnings,
        StartupShutdownErrorsWarningsDuration,
        All
    }
}
