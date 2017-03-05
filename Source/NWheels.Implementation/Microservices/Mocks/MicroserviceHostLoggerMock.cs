using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices.Mocks
{
    internal class MicroserviceHostLoggerMock : IMicroserviceHostLogger
    {
        public IDisposable NodeActivating()
        {
            return new DisposableMock();
        }

        public void NodeActivationError(Exception e)
        {
        }

        public void NodeConfigureError(Exception e)
        {
        }

        public IDisposable NodeConfiguring()
        {
            return new DisposableMock();
        }

        public void NodeDeactivated()
        {
        }

        public IDisposable NodeDeactivating()
        {
            return new DisposableMock();
        }

        public void NodeDeactivationError(Exception e)
        {
        }

        public void NodeHasFailedToActivate(Exception e)
        {
        }

        public Exception NodeHasFailedToActivate()
        {
            throw new NotImplementedException();
        }

        public void NodeHasFailedToConfigure(Exception e)
        {
        }

        public Exception NodeHasFailedToConfigure()
        {
            throw new NotImplementedException();
        }

        public Exception NodeHasFailedToDeactivate()
        {
            throw new NotImplementedException();
        }

        public void NodeHasFailedToLoad(Exception e)
        {
        }

        public Exception NodeHasFailedToLoad()
        {
            throw new NotImplementedException();
        }

        public Exception NodeHasFailedToUnload()
        {
            throw new NotImplementedException();
        }

        public void NodeLoadError(Exception e)
        {
        }

        public IDisposable NodeLoading()
        {
            return new DisposableMock();
        }

        public IDisposable NodeShuttingDown()
        {
            return new DisposableMock();
        }

        public IDisposable NodeStartingUp()
        {
            return new DisposableMock();
        }

        public void NodeSuccessfullyActivated()
        {
        }

        public void NodeSuccessfullyLoaded()
        {
        }

        public void NodeUnloaded()
        {
        }

        public void NodeUnloadError(Exception e)
        {
        }

        public IDisposable NodeUnloading()
        {
            return new DisposableMock();
        }

        public IDisposable LookingForLifecycleComponents()
        {
            return new DisposableMock();
        }

        public void FoundLifecycleComponent(string component)
        { }

        public void NoLifecycleComponentsFound()
        { }

        public void FailedToLoadLifecycleComponents(Exception e)
        { }

        public class DisposableMock : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
