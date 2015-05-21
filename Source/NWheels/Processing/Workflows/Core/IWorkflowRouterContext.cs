namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowRouterContext : IWorkflowActorSiteContext
    {
        bool HasActorWorkItem<TWorkItem>();
        TWorkItem GetActorWorkItem<TWorkItem>();
        bool HasActorResult<TResult>();
        TResult GetActorResult<TResult>();
        bool HasReceivedEvent<TEvent>() where TEvent : IWorkflowEvent;
        TEvent GetReceivedEvent<TEvent>() where TEvent : IWorkflowEvent;
    }
}
