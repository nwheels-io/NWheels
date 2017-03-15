using NWheels.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices.Mocks
{
    public class MicroserviceHostLoggerMock : IMicroserviceHostLogger
    {
        private List<string> _logs;

        public MicroserviceHostLoggerMock()
        {
            _logs = new List<string>();
        }

        public ILogActivity NodeActivating()
        {
            return new LogActivityMock();
        }

        public void NodeActivationError(Exception e)
        {
            Console.WriteLine(e);
        }

        public void NodeConfigureError(Exception e)
        {
            Console.WriteLine(e);
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
            Console.WriteLine(e);
        }

        public void NodeHasFailedToActivate(Exception e)
        {
            Console.WriteLine(e);
        }

        public Exception NodeHasFailedToActivate()
        {
            throw new NotImplementedException();
        }

        public void NodeHasFailedToConfigure(Exception e)
        {
            Console.WriteLine(e);
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
            Console.WriteLine(e);
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
            Console.WriteLine(e);
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
            Console.WriteLine(e);
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

        public void FoundFeatureLoaderComponent(string component)
        {
            Log(nameof(this.FoundFeatureLoaderComponent), nameof(component), component);
        }

        public void NoLifecycleComponentsFound()
        { }

        public void FailedToLoadLifecycleComponents(Exception e)
        {
            Console.WriteLine(e);
        }

        public void ComponentsEventFailed(Type component, string @event, Exception e)
        {
            Console.WriteLine(e);
        }

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

        public string[] TakeLog()
        {
            var result = _logs.ToArray();
            _logs.Clear();
            return result;
        }

        private void Log(string methodName, params string[] args)
        {
            var arguments = new StringBuilder();
            for (var i = 0; i < args.Length / 2; i++)
            {
                arguments.Append($"{args[i*2]}={args[i*2 + 1]}");
            }
            _logs.Add($"{methodName}({arguments})");
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
