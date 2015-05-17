namespace NWheels.Processing.Core
{
    public interface IWorkflowActorSiteContext
    {
        void EnqueueWorkItem<TWorkItem>(string actorName, TWorkItem workItem);
        IWorkflowInstance WorkflowInstance { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowActorContext : IWorkflowActorSiteContext
    {
        void SetResult<T>(T resultValue);
        void AwaitEvent<TEvent>() where TEvent : IWorkflowEvent;
        void AwaitEvent<TEvent, TKey>(TKey key) where TEvent : IWorkflowEvent<TKey>;
    }
}
