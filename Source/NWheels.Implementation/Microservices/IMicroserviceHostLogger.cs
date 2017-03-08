using NWheels.Logging;
using System;

namespace NWheels.Microservices
{
    public interface IMicroserviceHostLogger
    {
        ILogActivity NodeConfiguring();

        ILogActivity NodeLoading();

        void NodeHasFailedToConfigure(Exception e);

        Exception NodeHasFailedToConfigure();

        void NodeHasFailedToLoad(Exception e);

        Exception NodeHasFailedToLoad();

        void NodeSuccessfullyLoaded();

        ILogActivity NodeActivating();

        void NodeHasFailedToActivate(Exception e);

        Exception NodeHasFailedToActivate();

        void NodeSuccessfullyActivated();

        ILogActivity NodeStartingUp();

        ILogActivity NodeDeactivating();

        Exception NodeHasFailedToDeactivate();

        void NodeDeactivated();

        ILogActivity NodeShuttingDown();

        ILogActivity NodeUnloading();

        Exception NodeHasFailedToUnload();

        void NodeUnloaded();

        void NodeConfigureError(Exception e);

        void NodeLoadError(Exception e);

        void NodeActivationError(Exception e);

        void NodeDeactivationError(Exception e);

        void NodeUnloadError(Exception e);

        ILogActivity LookingForLifecycleComponents();

        void FoundLifecycleComponent(string component);

        void FoundFeatureLoaderComponent(string component);

        void NoLifecycleComponentsFound();

        void FailedToLoadLifecycleComponents(Exception e);

        void ComponentsEventFailed(Type component, string @event, Exception error);

        ILogActivity MicroserviceLoading(string component);

        ILogActivity MicroserviceUnloaded(string component);

        ILogActivity MicroserviceLoad(string component);

        ILogActivity MicroserviceUnload(string component);

        ILogActivity MicroserviceLoaded(string component);

        ILogActivity MicroserviceUnloading(string component);

        ILogActivity MicroserviceActivating(string component);

        ILogActivity MicroserviceDeactivated(string component);

        ILogActivity MicroserviceActivate(string component);

        ILogActivity MicroserviceDeactivate(string component);

        ILogActivity MicroserviceActivated(string component);

        ILogActivity MicroserviceDeactivating(string component);
    }
}
