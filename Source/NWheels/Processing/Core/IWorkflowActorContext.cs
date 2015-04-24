namespace NWheels.Processing.Core
{
    public interface IWorkflowActorContext
    {
        void EnqueueWorkItem<TWorkItem>(string actorName, TWorkItem workItem);
        void SetResult<T>(T resultValue);
        void AwaitEvent<TEvent>() where TEvent : IWorkflowEvent;
        void AwaitEvent<TEvent, TKey>(TKey key) where TEvent : IWorkflowEvent<TKey>;
    }
}
