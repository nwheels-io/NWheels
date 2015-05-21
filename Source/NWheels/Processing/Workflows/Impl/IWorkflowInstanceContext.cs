using System;
using Autofac;

namespace NWheels.Processing.Workflows.Impl
{
    internal interface IWorkflowInstanceContext
    {
        void AwaitEvent(Type eventType, object eventKey, TimeSpan timeout);
        IComponentContext Components { get; }
        IFramework Framework { get; }
        WorkflowCodeBehindAdapter CodeBehindAdapter { get; }
        IWorkflowInstanceEntity InstanceData { get; }
    }
}
