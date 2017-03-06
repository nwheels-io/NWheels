using NWheels.Logging;
using System;

namespace NWheels.Microservices.Mocks
{
    internal class MicroserviceHostLoggerMock : IMicroserviceHostLogger
    {
        public ILogActivity NodeActivating()
        {
            return new LogActivityMock();
        }

        public void NodeActivationError(Exception e)
        {
        }

        public void NodeConfigureError(Exception e)
        {
        }

        public ILogActivity NodeConfiguring()
        {
            return new LogActivityMock();
        }

        public void NodeDeactivated()
        {
        }

        public ILogActivity NodeDeactivating()
        {
            return new LogActivityMock();
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

        public ILogActivity NodeLoading()
        {
            return new LogActivityMock();
        }

        public ILogActivity NodeShuttingDown()
        {
            return new LogActivityMock();
        }

        public ILogActivity NodeStartingUp()
        {
            return new LogActivityMock();
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

        public ILogActivity NodeUnloading()
        {
            return new LogActivityMock();
        }

        public ILogActivity LookingForLifecycleComponents()
        {
            return new LogActivityMock();
        }

        public void FoundLifecycleComponent(string component)
        { }

        public void NoLifecycleComponentsFound()
        { }

        public void FailedToLoadLifecycleComponents(Exception e)
        { }

        public void ComponentsEventFailed(Type component, string @event, Exception error)
        { }

        public ILogActivity MicroserviceLoading(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceUnloaded(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceLoad(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceUnload(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceLoaded(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceUnloading(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceActivating(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceDeactivated(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceActivate(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceDeactivate(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceActivated(string component)
        {
            return new LogActivityMock();
        }

        public ILogActivity MicroserviceDeactivating(string component)
        {
            return new LogActivityMock();
        }

        public class LogActivityMock : ILogActivity
        {
            public void Dispose()
            {
            }

            public void Warn(Exception error)
            { }

            public void Fail(Exception error)
            { }
        }
    }
}
