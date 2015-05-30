using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Entities;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    [TestFixture]
    public class WorkflowInstanceTests : UnitTestBase
    {
        [Test]
        public void CanRunSingleActorWorkflow()
        {
            //-- arrange
            
            var environment = TestWorkflows.BuildSingleActorWorkflow(Framework);

            //-- act

            var workflowState = environment.InstanceUnderTest.Run();

            //-- assert

            Assert.That(workflowState, Is.EqualTo(WorkflowState.Completed));
            Assert.That(environment.InstanceData.WorkflowState, Is.EqualTo(WorkflowState.Completed));

            LogAssert.That(Framework.GetLog()).HasNoErrorsOrWarnings();
            LogAssert.That(Framework.GetLog()).Matches(Logex.Begin()
                .ZeroOrMore().AnyMessage()
                .One().Message<IWorkflowEngineLogger>(x => x.ProcessorRunning())
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor("A1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter("A1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExitingProcessorRun(ProcessorResult.Completed))
                .End());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSuspendAsyncWorkflowAwaitingForEvent()
        {
            //-- arrange

            var environment = TestWorkflows.BuildSingleEventAsyncWorkflow(Framework);

            //-- act

            var workflowState = environment.InstanceUnderTest.Run();

            //-- assert

            Assert.That(workflowState, Is.EqualTo(WorkflowState.Suspended));
            Assert.That(environment.InstanceData.WorkflowState, Is.EqualTo(WorkflowState.Suspended));

            LogAssert.That(Framework.GetLog()).HasNoErrorsOrWarnings();
            LogAssert.That(Framework.GetLog()).Matches(Logex.Begin()
                .ZeroOrMore().AnyMessage()
                .One().Message<IWorkflowEngineLogger>(x => x.ProcessorRunning())
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor("A1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter("A1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor("E1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExitingProcessorRun(ProcessorResult.Suspended))
                .End());

            Assert.That(environment.AwaitEventRequests.Count, Is.EqualTo(1));
            Assert.That(environment.AwaitEventRequests[0].EventType, Is.EqualTo(typeof(TestWorkflows.EventOne)));
            Assert.That(environment.AwaitEventRequests[0].EventKey, Is.EqualTo("K1"));
            Assert.That(environment.AwaitEventRequests[0].Timeout, Is.EqualTo(TimeSpan.FromMinutes(31)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, Ignore("WIP")]
        public void CanDispatchEventAndRunAsyncWorkflowToEnd()
        {
            //-- arrange

            var environment1 = TestWorkflows.BuildSingleEventAsyncWorkflow(Framework);
            var workflowState1 = environment1.InstanceUnderTest.Run();
            var log1 = Framework.TakeLog();

            var environment2 = TestWorkflows.BuildSingleEventAsyncWorkflow(Framework, environment1.InstanceData);
            var receivedEvent = new TestWorkflows.EventOne("K1", "ABC");

            //-- act

            var workflowState2 = environment2.InstanceUnderTest.DispatchAndRun(new IWorkflowEvent[] { receivedEvent });
            var log2 = Framework.TakeLog();

            //-- assert

            Assert.That(workflowState1, Is.EqualTo(WorkflowState.Suspended));
            Assert.That(workflowState2, Is.EqualTo(WorkflowState.Completed));
            Assert.That(environment2.InstanceData.WorkflowState, Is.EqualTo(WorkflowState.Completed));

            LogAssert.That(log1).HasNoErrorsOrWarnings();
            LogAssert.That(log2).HasNoErrorsOrWarnings();
            LogAssert.That(log2).Matches(Logex.Begin()
                .ZeroOrMore().AnyMessage()
                .One().Message<IWorkflowEngineLogger>(x => x.ProcessorRunning())
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter("E1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor("B1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter("B1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExitingProcessorRun(ProcessorResult.Completed))
                .End());

            Assert.That(environment2.AwaitEventRequests.Count, Is.EqualTo(0));
        }
    }
}
