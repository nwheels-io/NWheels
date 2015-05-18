using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing
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

    public interface IInitializableWorkflowCodeBehind<TDataEntity>
        where TDataEntity : class, IWorkflowInstanceEntity
    {
        void OnInitialize(TDataEntity initialData);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISuspendableWorkflowCodeBehind<TDataEntity> : IWorkflowCodeBehind
        where TDataEntity : class, IWorkflowInstanceEntity
    {
        void OnSuspend(TDataEntity dataToSave);
        void OnResume(TDataEntity savedData);
    }
}
