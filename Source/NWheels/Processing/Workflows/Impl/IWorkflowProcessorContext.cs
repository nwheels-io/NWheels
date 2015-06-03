using System;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows.Impl
{
    internal interface IWorkflowProcessorContext
    {
        void AwaitEvent(Type eventType, object eventKey, TimeSpan timeout);
        Guid WorkflowInstanceId { get; }
        IWorkflowInstanceInfo WorkflowInstance { get; }
        IWorkflowInstanceEntity InstanceData { get; }
        object InitialWorkItem { get; }
        IWorkflowEngineLogger Logger { get; }
    }
}
