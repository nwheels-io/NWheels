using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing
{
    public interface IWorkflowEngine
    {
        IWorkflowInstance StartWorkflow<TCodeBehind, TDataEntity>(TDataEntity initialData)
            where TDataEntity : class, IWorkflowInstanceEntity
            where TCodeBehind : class, IWorkflowCodeBehind;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IWorkflowInstance StartWorkflow(Type codeBehindType, IWorkflowInstanceEntity initialData);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void DispatchEvent(IWorkflowEvent receivedEvent);
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void DispatchEvents(IEnumerable<IWorkflowEvent> receivedEvents);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool TryGetWorkflow(Guid instanceId, out IWorkflowInstance instance);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        IWorkflowInstance[] GetCurrentWorkflows();
    }
}
