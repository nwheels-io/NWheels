namespace NWheels.Processing.Workflows
{
    public abstract class AbstractWorkflow : IWorkflowCodeBehind
    {
        public abstract void OnBuildWorkflow(IWorkflowBuilder builder);
    }
}
