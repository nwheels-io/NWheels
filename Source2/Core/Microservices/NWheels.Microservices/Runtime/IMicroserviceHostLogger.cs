using NWheels.Kernel.Api.Execution;
using System;

namespace NWheels.Microservices.Runtime
{
    public interface IMicroserviceHostLogger
    {
        IExecutionPathActivity NodeConfiguring();

        IExecutionPathActivity NodeLoading();

        void NodeHasFailedToConfigure(Exception e);

        Exception NodeHasFailedToConfigure();

        void NodeHasFailedToLoad(Exception e);

        Exception NodeHasFailedToLoad();

        void NodeSuccessfullyLoaded();

        IExecutionPathActivity NodeActivating();

        void NodeHasFailedToActivate(Exception e);

        Exception NodeHasFailedToActivate();

        void NodeSuccessfullyActivated();

        IExecutionPathActivity NodeStartingUp();

        IExecutionPathActivity NodeDeactivating();

        Exception NodeHasFailedToDeactivate();

        void NodeDeactivated();

        IExecutionPathActivity NodeShuttingDown();

        IExecutionPathActivity NodeUnloading();

        Exception NodeHasFailedToUnload();

        void NodeUnloaded();

        void NodeConfigureError(Exception e);

        void NodeLoadError(Exception e);

        void NodeActivationError(Exception e);

        void NodeDeactivationError(Exception e);

        void NodeUnloadError(Exception e);

        IExecutionPathActivity LookingForLifecycleComponents();

        void FoundLifecycleComponent(string component);

        void FoundFeatureLoaderComponent(string component);

        void NoLifecycleComponentsFound();

        void FailedToLoadLifecycleComponents(Exception e);

        void ComponentsEventFailed(Type component, string @event, Exception error);

        IExecutionPathActivity MicroserviceLoading(string component);

        IExecutionPathActivity MicroserviceUnloaded(string component);

        IExecutionPathActivity MicroserviceLoad(string component);

        IExecutionPathActivity MicroserviceUnload(string component);

        IExecutionPathActivity MicroserviceLoaded(string component);

        IExecutionPathActivity MicroserviceUnloading(string component);

        IExecutionPathActivity MicroserviceActivating(string component);

        IExecutionPathActivity MicroserviceDeactivated(string component);

        IExecutionPathActivity MicroserviceActivate(string component);

        IExecutionPathActivity MicroserviceDeactivate(string component);

        IExecutionPathActivity MicroserviceActivated(string component);

        IExecutionPathActivity MicroserviceDeactivating(string component);
    }
}
