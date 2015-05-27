using System;
using System.Collections.Generic;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows
{
    public interface IWorkflowEngine
    {
        IWorkflowInstanceInfo StartWorkflow<TCodeBehind, TDataEntity>(TDataEntity initialData)
            where TDataEntity : class, IWorkflowInstanceEntity
            where TCodeBehind : class, IWorkflowCodeBehind;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IWorkflowInstanceInfo StartWorkflow(Type codeBehindType, IWorkflowInstanceEntity initialData);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void DispatchEvent(IWorkflowEvent receivedEvent);
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void DispatchEvents(IEnumerable<IWorkflowEvent> receivedEvents);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void SendTrigger<TTrigger>(TTrigger trigger, Guid stateMachineInstanceId, object context = null);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool TryGetWorkflow(Guid instanceId, out IWorkflowInstanceInfo instance);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        IWorkflowInstanceInfo[] GetCurrentWorkflows();
    }
}
