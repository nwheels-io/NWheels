using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
{
    public enum ProcessorResult
    {
        Completed = WorkflowState.Completed,
        Suspended = WorkflowState.Suspended
    }
}
