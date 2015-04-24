namespace NWheels.Processing.Core
{
    public interface IWorkflowEvent
    {
        object KeyObject { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowEvent<out TKey> : IWorkflowEvent
    {
        TKey Key { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowEvent<out TKey, out TPayload> : IWorkflowEvent<TKey>
    {
        TPayload Payload { get; }
    }
}
