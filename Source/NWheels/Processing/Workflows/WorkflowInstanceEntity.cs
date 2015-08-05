using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows
{
    public abstract class WorkflowInstanceEntity : IWorkflowInstanceEntity
    {
        #region Implementation of IWorkflowInstanceEntity

        public abstract Guid WorkflowInstanceId { get; set; }
        public abstract Guid CorrelationId { get; set; }
        public abstract WorkflowState WorkflowState { get; set; }
        public abstract DateTime CreatedAtUtc { get; set; }
        public abstract DateTime UpdatedAtUtc { get; set; }
        public abstract DateTime TimeoutAtUtc { get; set; }
        public abstract Type CodeBehindClrType { get; set; }
        public abstract byte[] ProcessorSnapshot { get; set; }
        public abstract TimeSpan TotalTime { get; set; }
        public abstract TimeSpan TotalExecutionTime { get; set; }
        public abstract TimeSpan TotalSuspensionTime { get; set; }
        public abstract int TotalSuspensionCount { get; set; }

        #endregion
    }
}
