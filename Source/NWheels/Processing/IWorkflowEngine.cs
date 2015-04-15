using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IWorkflowEngine
    {
        IWorkflowInstance CreateWorkflow<TCodeBehind, TInstanceData>();
        IWorkflowInstance[] GetRunningWorkflows();

    }
}
