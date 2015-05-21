using System;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowReadyQueue
    {
        void EnqueueRun(Guid workflowInstanceId);
        void EnqueueDispatchAndRun(Guid workflowInstanceId, IWorkflowEvent[] events);
    }
}
