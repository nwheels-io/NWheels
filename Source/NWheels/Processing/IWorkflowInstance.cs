using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IWorkflowInstance
    {
        Guid InstanceId { get; }
        Type CodeBehindType { get; }
        WorkflowState State { get; }
        DateTime CreatedAtUtc { get; }
        DateTime StateChangedAtUtc { get; }
        TimeSpan TotalTime { get; }
        TimeSpan TotalExecutionTime { get; }
        TimeSpan TotalSuspensionTime { get; }
    }
}
