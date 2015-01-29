using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IApplicationJobContext
    {
        void Report(string statusText, decimal percentCompleted);
        bool IsDryRun { get; }
        CancellationToken Cancellation { get; }
    }
}
