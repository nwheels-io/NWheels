using System;

namespace NWheels.Microservices
{
    public interface IMicroserviceHostLogger
    {
        IDisposable NodeConfiguring();

        IDisposable NodeLoading();

        void NodeHasFailedToConfigure(Exception e);

        Exception NodeHasFailedToConfigure();

        void NodeHasFailedToLoad(Exception e);

        Exception NodeHasFailedToLoad();

        void NodeSuccessfullyLoaded();

        IDisposable NodeActivating();

        void NodeHasFailedToActivate(Exception e);

        Exception NodeHasFailedToActivate();

        void NodeSuccessfullyActivated();

        IDisposable NodeStartingUp();

        IDisposable NodeDeactivating();

        Exception NodeHasFailedToDeactivate();

        void NodeDeactivated();

        IDisposable NodeShuttingDown();

        IDisposable NodeUnloading();

        Exception NodeHasFailedToUnload();

        void NodeUnloaded();

        void NodeConfigureError(Exception e);

        void NodeLoadError(Exception e);

        void NodeActivationError(Exception e);

        void NodeDeactivationError(Exception e);

        void NodeUnloadError(Exception e);
    }
}
