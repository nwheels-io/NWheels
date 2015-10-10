using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Stacks.ODataBreeze
{
    public interface IBreezeEndpointLogger : IApplicationEventLogger
    {
        [LogActivity]
        ILogActivity RestWriteInProgress();

        [LogInfo]
        void RestWriteCompleted();

        [LogError]
        void RestWriteFailed(Exception e);
    }
}
