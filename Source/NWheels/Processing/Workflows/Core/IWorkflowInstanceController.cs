using System.Collections.Generic;
using NWheels.Processing.Core;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowInstanceController
    {
        WorkflowState Run();
        WorkflowState DispatchAndRun(IEnumerable<IWorkflowEvent> receivedEvents);
        IWorkflowInstance InstanceContext { get; }
    }
}
