namespace NWheels.Processing.Core
{
    public interface IWorkflowActor
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowActor<TWorkItem> : IWorkflowActor
    {
        void Execute(IWorkflowActorContext context, TWorkItem workItem);
    }
}
