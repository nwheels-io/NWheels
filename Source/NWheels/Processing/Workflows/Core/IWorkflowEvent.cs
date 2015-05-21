using System;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowEvent
    {
        Type GetEventType();
        object GetEventKey();
        WorkflowEventStatus GetEventStatus();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowEvent<out TKey> : IWorkflowEvent
    {
        TKey Key { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public enum WorkflowEventStatus
    {
        Received,
        TimedOut
    }
}
