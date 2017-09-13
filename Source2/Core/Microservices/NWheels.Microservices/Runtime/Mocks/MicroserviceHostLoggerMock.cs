using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NWheels.Kernel.Api.Execution;

namespace NWheels.Microservices.Runtime.Mocks
{
    public class MicroserviceHostLoggerMock : IMicroserviceHostLogger
    {
        private List<string> _logs;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostLoggerMock()
        {
            _logs = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity NodeActivating()
        {
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeActivationError(Exception e)
        {
            Console.WriteLine($"{nameof(NodeActivationError)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeConfigureError(Exception e)
        {
            Console.WriteLine($"{nameof(NodeConfigureError)} : {e}");

            if (e is ReflectionTypeLoadException loadException)
            {
                foreach (var e2 in loadException.LoaderExceptions)
                {
                    Console.WriteLine(e2);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity NodeConfiguring()
        {
            Console.WriteLine($"{nameof(NodeConfiguring)}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeDeactivated()
        {
            Console.WriteLine($"{nameof(NodeDeactivated)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity NodeDeactivating()
        {
            Console.WriteLine($"{nameof(NodeDeactivating)}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeDeactivationError(Exception e)
        {
            Console.WriteLine($"{nameof(NodeDeactivationError)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeHasFailedToActivate(Exception e)
        {
            Console.WriteLine($"{nameof(NodeHasFailedToActivate)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception NodeHasFailedToActivate()
        {
            Console.WriteLine($"{nameof(NodeHasFailedToActivate)}");
            return new Exception($"{nameof(NodeHasFailedToActivate)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeHasFailedToConfigure(Exception e)
        {
            Console.WriteLine($"{nameof(NodeHasFailedToConfigure)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception NodeHasFailedToConfigure()
        {
            Console.WriteLine($"{nameof(NodeHasFailedToConfigure)}");
            return new Exception($"{nameof(NodeHasFailedToConfigure)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception NodeHasFailedToDeactivate()
        {
            Console.WriteLine($"{nameof(NodeHasFailedToDeactivate)}");
            return new Exception($"{nameof(NodeHasFailedToDeactivate)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeHasFailedToLoad(Exception e)
        {
            Console.WriteLine($"{nameof(NodeHasFailedToLoad)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception NodeHasFailedToLoad()
        {
            Console.WriteLine($"{nameof(NodeHasFailedToLoad)}");
            return new Exception($"{nameof(NodeHasFailedToLoad)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception NodeHasFailedToUnload()
        {
            Console.WriteLine($"{nameof(NodeHasFailedToUnload)}");
            return new Exception($"{nameof(NodeHasFailedToUnload)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeLoadError(Exception e)
        {
            Console.WriteLine($"{nameof(NodeLoadError)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity NodeLoading()
        {
            Console.WriteLine($"{nameof(NodeLoading)}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity NodeShuttingDown()
        {
            Console.WriteLine($"{nameof(NodeShuttingDown)}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity NodeStartingUp()
        {
            Console.WriteLine($"{nameof(NodeStartingUp)}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeSuccessfullyActivated()
        {
            Console.WriteLine($"{nameof(NodeSuccessfullyActivated)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeSuccessfullyLoaded()
        {
            Console.WriteLine($"{nameof(NodeSuccessfullyLoaded)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeUnloaded()
        {
            Console.WriteLine($"{nameof(NodeUnloaded)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NodeUnloadError(Exception e)
        {
            Console.WriteLine($"{nameof(NodeUnloadError)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity NodeUnloading()
        {
            Console.WriteLine($"{nameof(NodeUnloading)}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity LookingForLifecycleComponents()
        {
            Console.WriteLine($"{nameof(LookingForLifecycleComponents)}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FoundLifecycleComponent(string component)
        {
            Console.WriteLine($"{nameof(FoundLifecycleComponent)} : {component}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FoundFeatureLoaderComponent(string component)
        {
            Console.WriteLine($"{nameof(FoundFeatureLoaderComponent)} : {component}");
            Log(nameof(this.FoundFeatureLoaderComponent), nameof(component), component);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NoLifecycleComponentsFound()
        {
            Console.WriteLine($"{nameof(NoLifecycleComponentsFound)}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FailedToLoadLifecycleComponents(Exception e)
        {
            Console.WriteLine($"{nameof(FailedToLoadLifecycleComponents)} : {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ComponentsEventFailed(Type component, string @event, Exception e)
        {
            Console.WriteLine($"{nameof(ComponentsEventFailed)} : {component}, {@event}, {e}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceLoading(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceLoading)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceUnloaded(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceUnloaded)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceLoad(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceLoad)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceUnload(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceUnload)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceLoaded(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceUnload)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceUnloading(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceUnloading)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceActivating(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceActivating)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceDeactivated(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceDeactivated)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceActivate(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceActivate)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceDeactivate(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceDeactivate)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceActivated(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceActivated)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IExecutionPathActivity MicroserviceDeactivating(string component)
        {
            Console.WriteLine($"{nameof(MicroserviceDeactivating)} : {component}");
            return new LogActivityMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] TakeLog()
        {
            var result = _logs.ToArray();
            _logs.Clear();
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Log(string methodName, params string[] args)
        {
            var arguments = new StringBuilder();
            for (var i = 0; i < args.Length / 2; i++)
            {
                arguments.Append($"{args[i*2]}={args[i*2 + 1]}");
            }
            _logs.Add($"{methodName}({arguments})");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LogActivityMock : IExecutionPathActivity
        {
            public void Dispose()
            {
            }

            public void Warn(Exception error)
            {
            }

            public void Warn(string reason)
            {
            }

            public void Fail(Exception error)
            {
            }

            public void Fail(string reason)
            {
                throw new NotImplementedException();
            }
        }
    }
}
