using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    [TestFixture]
    public class StateMachineWorkflowTests : UnitTestBase
    {
        [Test, Ignore("WIP")]
        public void CanStartStateMachineWorkflow()
        {
            //-- arrange

            var environment = TestWorkflows.BuildStateMachineWorkflow(Framework);

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
                .One().Message<IWorkflowEngineLogger>(x => x.ExitingProcessorRun(ProcessorResult.Completed))
                .End());
        }
    }
}
