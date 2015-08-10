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
    public class StateMachineWorkflowTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanStartStateMachineWorkflow()
        {
            //-- arrange

            var environment = TestWorkflows.BuildStateMachineWorkflow(Framework);

            //-- act

            var workflowState = environment.InstanceUnderTest.Run();

            //-- assert

            Assert.That(environment.InstanceData.MachineState, Is.EqualTo(TestWorkflows.OrderItemState.PaymentConfirmationPending));
            Assert.That(environment.InstanceData.WorkflowState, Is.EqualTo(WorkflowState.Suspended));
            Assert.That(workflowState, Is.EqualTo(WorkflowState.Suspended));

            LogAssert.That(Framework.GetLog()).HasNoErrorsOrWarnings();
            LogAssert.That(Framework.GetLog()).Matches(Logex.Begin()
                .One().Message<IWorkflowEngineLogger>(x => x.ProcessorRunning())
                .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor(TestWorkflows.OrderItemState.PaymentConfirmationPending.ToString()))
                .One().Message<TestWorkflows.IOrderItemLogger>(x => x.PaymentConfirmationPending())
                .One().Message<IWorkflowEngineLogger>(x => x.ExitingProcessorRun(ProcessorResult.Suspended))
                .End());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanDispatchStateTransitionTrigger()
        {
            //-- arrange

            var environment1 = TestWorkflows.BuildStateMachineWorkflow(Framework);
            var workflowState1 = environment1.InstanceUnderTest.Run();
            var machineState1 = environment1.InstanceData.MachineState;
            var log1 = Framework.TakeLog();

            var environment2 = TestWorkflows.BuildStateMachineWorkflow(Framework, environment1.InstanceData);
            var receivedEvent = new StateMachineTriggerEvent<TestWorkflows.OrderItemTrigger>(
                environment1.InstanceData.WorkflowInstanceId, 
                TestWorkflows.OrderItemTrigger.PaymentReceived, 
                null);

            //-- act

            var workflowState2 = environment2.InstanceUnderTest.DispatchAndRun(new[] { receivedEvent });
            var machineState2 = environment2.InstanceData.MachineState;
            var log2 = Framework.TakeLog();

            //-- assert

            Assert.That(machineState1, Is.EqualTo(TestWorkflows.OrderItemState.PaymentConfirmationPending));
            Assert.That(workflowState1, Is.EqualTo(WorkflowState.Suspended));
            Assert.That(machineState2, Is.EqualTo(TestWorkflows.OrderItemState.LockingStock));
            Assert.That(workflowState2, Is.EqualTo(WorkflowState.Suspended));
            Assert.That(environment2.InstanceData.WorkflowState, Is.EqualTo(WorkflowState.Suspended));

            LogAssert.That(log1).HasNoErrorsOrWarnings();
            LogAssert.That(log2).HasNoErrorsOrWarnings();
            LogAssert.That(log2).Matches(Logex
                .Begin()
                    .One().Message<IWorkflowEngineLogger>(x => x.ProcessorDispatchingEvent(
                        typeof(StateMachineTriggerEvent<TestWorkflows.OrderItemTrigger>),
                        environment2.InstanceData.WorkflowInstanceId, 
                        WorkflowEventStatus.Received,
                        TestWorkflows.OrderItemState.PaymentConfirmationPending.ToString()))
                    .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter(TestWorkflows.OrderItemState.PaymentConfirmationPending.ToString()))
                    .One().Message<IWorkflowEngineLogger>(x => x.ProcessorRunning())
                    .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor(TestWorkflows.OrderItemState.LockingStock.ToString()))
                    .One().Message<TestWorkflows.IOrderItemLogger>(x => x.LockingStock())
                    .One().Message<IWorkflowEngineLogger>(x => x.ExitingProcessorRun(ProcessorResult.Suspended))
                .End());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCompleteStateMachineWorkflow()
        {
            //-- arrange

            var environment0 = TestWorkflows.BuildStateMachineWorkflow(Framework);
            var machineState0 = environment0.InstanceUnderTest.Run();
            var instanceData = environment0.InstanceData;
            var dispatchAndRun = new Action<TestWorkflows.OrderItemState, TestWorkflows.OrderItemTrigger>(
                (expectedState, trigger) => {
                    Assert.That(instanceData.MachineState, Is.EqualTo(expectedState));
                    TestWorkflows.BuildStateMachineWorkflow(Framework, instanceData).InstanceUnderTest.DispatchAndRun(new[] {
                        new StateMachineTriggerEvent<TestWorkflows.OrderItemTrigger>(instanceData.WorkflowInstanceId, trigger, null)
                    });
                });

            dispatchAndRun(TestWorkflows.OrderItemState.PaymentConfirmationPending, TestWorkflows.OrderItemTrigger.PaymentReceived);
            dispatchAndRun(TestWorkflows.OrderItemState.LockingStock, TestWorkflows.OrderItemTrigger.StockLocked);
            dispatchAndRun(TestWorkflows.OrderItemState.RequestingDelivery, TestWorkflows.OrderItemTrigger.DeliveryRequestAccepted);

            LogAssert.That(Framework.TakeLog()).HasNoErrorsOrWarnings();

            //-- act

            dispatchAndRun(TestWorkflows.OrderItemState.DeliveryConfirmationPending, TestWorkflows.OrderItemTrigger.DeliveryConfirmed);
            
            //-- assert

            var log = Framework.TakeLog();

            Assert.That(instanceData.MachineState, Is.EqualTo(TestWorkflows.OrderItemState.Delivered));
            Assert.That(instanceData.WorkflowState, Is.EqualTo(WorkflowState.Completed));

            LogAssert.That(log).Matches(Logex
                .Begin()
                    .One().Message<IWorkflowEngineLogger>(x => x.ProcessorDispatchingEvent(
                        typeof(StateMachineTriggerEvent<TestWorkflows.OrderItemTrigger>),
                        instanceData.WorkflowInstanceId,
                        WorkflowEventStatus.Received,
                        TestWorkflows.OrderItemState.DeliveryConfirmationPending.ToString()))
                    .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter(TestWorkflows.OrderItemState.DeliveryConfirmationPending.ToString()))
                    .One().Message<IWorkflowEngineLogger>(x => x.ProcessorRunning())
                    .One().Message<IWorkflowEngineLogger>(x => x.ExecutingActor(TestWorkflows.OrderItemState.Delivered.ToString()))
                    .One().Message<TestWorkflows.IOrderItemLogger>(x => x.Delivered())
                    .One().Message<IWorkflowEngineLogger>(x => x.ExecutingRouter(TestWorkflows.OrderItemState.Delivered.ToString()))
                    .One().Message<IWorkflowEngineLogger>(x => x.ExitingProcessorRun(ProcessorResult.Completed))
                .End());
        }
    }
}
