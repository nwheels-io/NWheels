using System;
using NWheels.Processing.Workflows.Core;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    internal class TestWorkflowInstanceInfo : IWorkflowInstanceInfo
    {
        #region Implementation of IWorkflowInstance

        public Guid InstanceId { get; set; }
        public WorkflowState State { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime StateChangedAtUtc { get; set; }
        public Type CodeBehindType { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public TimeSpan TotalSuspensionTime { get; set; }
        public int TotalSuspensionCount { get; set; }

        #endregion
    }
}