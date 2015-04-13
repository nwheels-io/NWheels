using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public interface IInitializableWorkflowCodeBehind<TInitData>
    {
        void OnInitialize(TInitData initializationData);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISuspendableWorkflowCodeBehind<TSnapshot>
    {
        void OnSuspend(out TSnapshot persistableSnapshot);
        void OnResume(TSnapshot persistedSnapshot);
    }
}
