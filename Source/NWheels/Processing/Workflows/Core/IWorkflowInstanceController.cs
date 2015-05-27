using System.Collections.Generic;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowInstanceController
    {
        WorkflowState Run();
        WorkflowState DispatchAndRun(IEnumerable<IWorkflowEvent> receivedEvents);
        IWorkflowInstanceInfo InstanceContext { get; }
    }
}
