using System;

namespace NWheels.Processing.Workflows
{
    public interface IWorkflowCodeBehind
    {
        void OnBuildWorkflow(IWorkflowBuilder builder);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowCodeBehindLifecycle
    {
        void OnStart();
        void OnComplete();
        void OnFail(Exception error);
        void OnFinalize();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowInitializer
    {
        void SetInitialWorkItem(object workItem);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IInitializableWorkflowCodeBehind<TDataEntity> : IWorkflowCodeBehind
        where TDataEntity : class, IWorkflowInstanceEntity
    {
        void OnInitialize(TDataEntity initialData, IWorkflowInitializer initializer);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISuspendableWorkflowCodeBehind<TDataEntity> : IWorkflowCodeBehind
        where TDataEntity : class, IWorkflowInstanceEntity
    {
        void OnSuspend(TDataEntity dataToSave);
        void OnResume(TDataEntity savedData);
    }
}
