using System;
using System.Collections.Generic;
using NWheels.Logging;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Processing.Workflows.Impl;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    internal class ProcessorTestEnvironment
    {
        public ProcessorTestEnvironment(TestFramework framework)
        {
            this.ProcessorContext = new TestProcessorContext(this);
            this.WorkflowInstanceInfo = new TestWorkflowInstanceInfo();
            this.ProcessorUnderTest = new WorkflowProcessor(framework, this.ProcessorContext);
            this.AwaitEventRequests = new List<AwaitEventRequest>();
            this.Logger = framework.Logger<IWorkflowEngineLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildWorkflow(Action<IWorkflowBuilder> callback)
        {
            callback(this.ProcessorUnderTest);
            this.ProcessorUnderTest.EndBuildWorkflow();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessorTestEnvironment AddActor<T>(
            string name, 
            int priority, 
            Action<IWorkflowActorContext, T> onExecute, 
            Action<IWorkflowRouterContext> onRoute, 
            bool isInitial = false)
        {
            ((IWorkflowBuilder)this.ProcessorUnderTest).AddActor(name, priority, new TestActor<T>(onExecute), new TestRouter(onRoute), isInitial);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EndBuildWorkflow()
        {
            this.ProcessorUnderTest.EndBuildWorkflow();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessorResult Run(object initialWorkItem, out WorkflowProcessorSnapshot snapshot)
        {
            InitialWorkItem = initialWorkItem;
            var result = ProcessorUnderTest.Run();
            snapshot = (result == ProcessorResult.Suspended ? ProcessorUnderTest.TakeSnapshot() : null);
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessorResult DispatchAndRun(ref WorkflowProcessorSnapshot snapshot, params IWorkflowEvent[] receivedEvents)
        {
            ProcessorUnderTest.RestoreSnapshot(snapshot);
            var result = ProcessorUnderTest.DispatchAndRun(receivedEvents);
            
            snapshot = (result == ProcessorResult.Suspended ? ProcessorUnderTest.TakeSnapshot() : null);
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestProcessorContext ProcessorContext { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestWorkflowInstanceInfo WorkflowInstanceInfo { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowProcessor ProcessorUnderTest { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object InitialWorkItem { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<AwaitEventRequest> AwaitEventRequests { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IWorkflowEngineLogger Logger { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ProcessorTestEnvironment Build(TestFramework framework, Action<ProcessorTestEnvironment> onBuild)
        {
            var environment = new ProcessorTestEnvironment(framework);
            onBuild(environment);
            environment.EndBuildWorkflow();
            return environment;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Func<ProcessorTestEnvironment> BuildFactory(TestFramework framework, Action<ProcessorTestEnvironment> onBuild)
        {
            return () => Build(framework, onBuild);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestProcessorContext : IWorkflowProcessorContext
        {
            private readonly ProcessorTestEnvironment _environment;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestProcessorContext(ProcessorTestEnvironment environment)
            {
                _environment = environment;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AwaitEvent(Type eventType, object eventKey, TimeSpan timeout)
            {
                _environment.AwaitEventRequests.Add(new AwaitEventRequest(eventType, eventKey, timeout));    
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IWorkflowInstanceInfo WorkflowInstance
            {
                get { return _environment.WorkflowInstanceInfo; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid WorkflowInstanceId
            {
                get { return WorkflowInstance.InstanceId; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object InitialWorkItem
            {
                get { return _environment.InitialWorkItem; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IWorkflowEngineLogger Logger
            {
                get { return _environment.Logger; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestActor<T> : IWorkflowActor<T>
        {
            private readonly Action<IWorkflowActorContext, T> _onExecute;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestActor(Action<IWorkflowActorContext, T> onExecute)
            {
                _onExecute = onExecute;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Execute(IWorkflowActorContext context, T workItem)
            {
                _onExecute(context, workItem);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestRouter : IWorkflowRouter
        {
            private readonly Action<IWorkflowRouterContext> _onRoute;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestRouter(Action<IWorkflowRouterContext> onRoute)
            {
                _onRoute = onRoute;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Route(IWorkflowRouterContext context)
            {
                _onRoute(context);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type GetCookieType()
            {
                return null;
            }
        }
    }
}
