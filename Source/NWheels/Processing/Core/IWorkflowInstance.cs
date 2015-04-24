using System;

namespace NWheels.Processing.Core
{
    public interface IWorkflowInstance
    {
        Guid InstanceId { get; }
        WorkflowState State { get; }
        DateTime CreatedAtUtc { get; }
        DateTime StateChangedAtUtc { get; }
        Type CodeBehindType { get; }
        TimeSpan TotalTime { get; }
        TimeSpan TotalExecutionTime { get; }
        TimeSpan TotalSuspensionTime { get; }
        int TotalSuspensionCount { get; }
    }
}
