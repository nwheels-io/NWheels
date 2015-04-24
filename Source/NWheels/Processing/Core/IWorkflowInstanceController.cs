using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
{
    public interface IWorkflowInstanceController
    {
        WorkflowState Run();
        WorkflowState DispatchAndRun(IEnumerable<IWorkflowEvent> receivedEvents);
        IWorkflowInstance InstanceContext { get; }
    }
}
