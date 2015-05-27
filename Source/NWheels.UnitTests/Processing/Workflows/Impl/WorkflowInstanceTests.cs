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
        [Test, Ignore("WIP")]
        public void CanRunSingleActorWorkflow()
        {
            //-- arrange
            
            var environment = TestWorkflows.BuildSingleActorWorkflow(Framework);

            //-- act

            environment.InstanceUnderTest.Run();

            //-- assert

            LogAssert.That(Framework.GetLog()).HasNoErrorsOrWarnings();
            LogAssert.That(Framework.GetLog()).Matches(Logex.Begin()
                .ZeroOrMore().AnyMessage()
                .One().Message<IWorkflowEngineLogger>(x => x.ProcessorRunning())
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor("A1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter("A1"))
                .One().Message<IWorkflowEngineLogger>(x => x.ExisingProcessorRun(ProcessorResult.Completed))
                .End());
        }
    }
}
