using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Processing.Impl
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
