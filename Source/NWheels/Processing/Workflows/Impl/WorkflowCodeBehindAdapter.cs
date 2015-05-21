using System;
using Autofac;

namespace NWheels.Processing.Workflows.Impl
{
    internal abstract class WorkflowCodeBehindAdapter
    {
        public abstract IWorkflowCodeBehind InstantiateCodeBehind(IComponentContext components);
        public abstract void OnBuildWorkflow(IWorkflowCodeBehind codeBehind, IWorkflowBuilder builder);
        public abstract void OnInitialize(IWorkflowCodeBehind codeBehind, IWorkflowInstanceEntity initialData, IWorkflowInitializer initializer);
        public abstract void OnStart(IWorkflowCodeBehind codeBehind);
        public abstract void OnSuspend(IWorkflowCodeBehind codeBehind, IWorkflowInstanceEntity dataToSave);
        public abstract void OnResume(IWorkflowCodeBehind codeBehind, IWorkflowInstanceEntity savedData);
        public abstract void OnComplete(IWorkflowCodeBehind codeBehind);
        public abstract void OnFail(IWorkflowCodeBehind codeBehind, Exception error);
        public abstract void OnFinalize(IWorkflowCodeBehind codeBehind);
        public abstract Type CodeBehindType { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static WorkflowCodeBehindAdapter Create(IComponentContext components, Type codeBehindType, Type dataEntityType)
        {
            var codeBehindInstance = components.Resolve(codeBehindType);
            var closedAdapterType = typeof(WorkflowCodeBehindAdapter<,>).MakeGenericType(codeBehindType, dataEntityType);
            var adapterInstance = Activator.CreateInstance(closedAdapterType, codeBehindInstance);

            return (WorkflowCodeBehindAdapter)adapterInstance;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal class WorkflowCodeBehindAdapter<TCodeBehind, TDataEntity> : WorkflowCodeBehindAdapter
        where TDataEntity : class, IWorkflowInstanceEntity
        where TCodeBehind : class, IWorkflowCodeBehind
    {
        public override IWorkflowCodeBehind InstantiateCodeBehind(IComponentContext components)
        {
            return components.Resolve<TCodeBehind>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnBuildWorkflow(IWorkflowCodeBehind codeBehind, IWorkflowBuilder builder)
        {
            codeBehind.OnBuildWorkflow(builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnInitialize(IWorkflowCodeBehind codeBehind, IWorkflowInstanceEntity initialData, IWorkflowInitializer initializer)
        {
            InvokeIf<IInitializableWorkflowCodeBehind<TDataEntity>>(
                codeBehind,
                initializable => {
                    initializable.OnInitialize((TDataEntity)initialData, initializer);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnStart(IWorkflowCodeBehind codeBehind)
        {
            InvokeIf<IWorkflowCodeBehindLifecycle>(
                codeBehind,
                lifecycle => {
                    lifecycle.OnStart();
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnSuspend(IWorkflowCodeBehind codeBehind, IWorkflowInstanceEntity dataToSave)
        {
            InvokeIf<ISuspendableWorkflowCodeBehind<TDataEntity>>(
                codeBehind,
                lifecycle => {
                    lifecycle.OnSuspend((TDataEntity)dataToSave);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnResume(IWorkflowCodeBehind codeBehind, IWorkflowInstanceEntity savedData)
        {
            InvokeIf<ISuspendableWorkflowCodeBehind<TDataEntity>>(
                codeBehind,
                lifecycle => {
                    lifecycle.OnResume((TDataEntity)savedData);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnComplete(IWorkflowCodeBehind codeBehind)
        {
            InvokeIf<IWorkflowCodeBehindLifecycle>(
                codeBehind,
                lifecycle => {
                    lifecycle.OnComplete();
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnFail(IWorkflowCodeBehind codeBehind, Exception error)
        {
            InvokeIf<IWorkflowCodeBehindLifecycle>(
                codeBehind,
                lifecycle => {
                    lifecycle.OnFail(error);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void OnFinalize(IWorkflowCodeBehind codeBehind)
        {
            InvokeIf<IWorkflowCodeBehindLifecycle>(
                codeBehind,
                lifecycle => {
                    lifecycle.OnFinalize();
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type CodeBehindType
        {
            get { return typeof(TCodeBehind); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InvokeIf<TCodeBehindService>(IWorkflowCodeBehind codeBehind, Action<TCodeBehindService> action) 
            where TCodeBehindService : class
        {
            var service = (codeBehind as TCodeBehindService);

            if ( service != null )
            {
                action(service);
            }
        }
    }
}
