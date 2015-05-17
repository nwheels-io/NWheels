using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public abstract class AbstractWorkflow : IWorkflowCodeBehind
    {
        public abstract void OnBuildWorkflow(IWorkflowBuilder builder);
    }
}
