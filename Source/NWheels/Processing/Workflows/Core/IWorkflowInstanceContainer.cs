using System;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowInstanceContainer
    {
        IWorkflowInstanceController GetInstanceById(Guid instanceId);
    }
}
