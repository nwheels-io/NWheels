using System;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowActorSiteContext
    {
        void EnqueueWorkItem<TWorkItem>(string actorName, TWorkItem workItem);
        IWorkflowInstance WorkflowInstance { get; }
        string ActorName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowActorContext : IWorkflowActorSiteContext
    {
        void SetResult<T>(T resultValue);
        void AwaitEvent<TEvent>(TimeSpan timeout) where TEvent : IWorkflowEvent;
        void AwaitEvent<TEvent, TKey>(TKey key, TimeSpan timeout) where TEvent : IWorkflowEvent<TKey>;
    }
}
