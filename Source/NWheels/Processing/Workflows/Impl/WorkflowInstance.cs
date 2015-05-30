using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NWheels.Processing.Workflows.Core;
using ProtoBuf;

namespace NWheels.Processing.Workflows.Impl
{
    internal class WorkflowInstance : 
        IWorkflowInstanceInfo, 
        IWorkflowProcessorContext,
        IWorkflowInitializer,
        IStateMachineCodeBehind<WorkflowProcessorState, WorkflowProcessorTrigger>
    {
        private readonly IWorkflowInstanceContext _context;
        private readonly IFramework _framework;
        private readonly WorkflowCodeBehindAdapter _codeBehindAdapter;
        private readonly IWorkflowInstanceEntity _instanceData;
        private readonly TransientStateMachine<WorkflowProcessorState, WorkflowProcessorTrigger> _processorStateMachine;
        private readonly IWorkflowEngineLogger _logger;
        private IWorkflowCodeBehind _codeBehindInstance;
        private object _initialWorkItem;
        private IWorkflowEvent[] _receivedEvents;
        private Exception _failureException;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowInstance(IWorkflowInstanceContext context)
        {
            _context = context;
            _framework = context.Framework;
            _codeBehindAdapter = context.CodeBehindAdapter;
            _instanceData = context.InstanceData;
            _processorStateMachine = new TransientStateMachine<WorkflowProcessorState, WorkflowProcessorTrigger>(this, _context.Components);
            _logger = _context.Components.Resolve<IWorkflowEngineLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStateMachineCodeBehind<WorkflowProcessorState, WorkflowProcessorTrigger>.BuildStateMachine(
            IStateMachineBuilder<WorkflowProcessorState, WorkflowProcessorTrigger> machine)
        {
            machine.State(WorkflowProcessorState.Created)
                .SetAsInitial()
                .OnTrigger(WorkflowProcessorTrigger.Run).TransitionTo(WorkflowProcessorState.Initializing)
                .OnTrigger(WorkflowProcessorTrigger.Resume).TransitionTo(WorkflowProcessorState.Resuming);

            machine.State(WorkflowProcessorState.Initializing)
                .OnEntered(OnProcessorInitializing)
                .OnTrigger(WorkflowProcessorTrigger.Success).TransitionTo(WorkflowProcessorState.Starting)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.FailedTerminated);

            machine.State(WorkflowProcessorState.Starting)
                .OnEntered(OnProcessorStarting)
                .OnTrigger(WorkflowProcessorTrigger.Success).TransitionTo(WorkflowProcessorState.Running)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.FailedTerminated);
            
            machine.State(WorkflowProcessorState.Resuming)
                .OnEntered(OnProcessorResuming)
                .OnTrigger(WorkflowProcessorTrigger.Success).TransitionTo(WorkflowProcessorState.Running)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.Failing);

            machine.State(WorkflowProcessorState.Running)
                .OnEntered(OnProcessorRunning)
                .OnTrigger(WorkflowProcessorTrigger.Suspended).TransitionTo(WorkflowProcessorState.Suspending)
                .OnTrigger(WorkflowProcessorTrigger.Completed).TransitionTo(WorkflowProcessorState.Completing)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.Failing);

            machine.State(WorkflowProcessorState.Suspending)
                .OnEntered(OnProcessorSuspending)
                .OnTrigger(WorkflowProcessorTrigger.Success).TransitionTo(WorkflowProcessorState.Suspended)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.Failing);

            machine.State(WorkflowProcessorState.Completing)
                .OnEntered(OnProcessorCompleting)
                .OnTrigger(WorkflowProcessorTrigger.Success).TransitionTo(WorkflowProcessorState.Finalizing)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.Finalizing);

            machine.State(WorkflowProcessorState.Failing)
                .OnEntered(OnProcessorFailing)
                .OnTrigger(WorkflowProcessorTrigger.Success).TransitionTo(WorkflowProcessorState.Finalizing)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.Finalizing);

            machine.State(WorkflowProcessorState.Finalizing)
                .OnEntered(OnProcessorFinalizing)
                .OnTrigger(WorkflowProcessorTrigger.Success).TransitionTo(WorkflowProcessorState.CompletedTerminated)
                .OnTrigger(WorkflowProcessorTrigger.Failure).TransitionTo(WorkflowProcessorState.FailedTerminated);

            machine.State(WorkflowProcessorState.Suspended).OnEntered(OnProcessorSuspended);
            machine.State(WorkflowProcessorState.CompletedTerminated).OnEntered(OnProcessorCompletedTerminated);
            machine.State(WorkflowProcessorState.FailedTerminated).OnEntered(OnProcessorFailedTerminated);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AwaitEvent(Type eventType, object eventKey, TimeSpan timeout)
        {
            _context.AwaitEvent(eventType, eventKey, timeout);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Guid IWorkflowProcessorContext.WorkflowInstanceId
        {
            get { return this.InstanceId; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IWorkflowInstanceInfo IWorkflowProcessorContext.WorkflowInstance
        {
            get { return this; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IWorkflowProcessorContext.InitialWorkItem
        {
            get { return _initialWorkItem; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IWorkflowEngineLogger IWorkflowProcessorContext.Logger
        {
            get { return _logger; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IWorkflowInitializer.SetInitialWorkItem(object workItem)
        {
            _initialWorkItem = workItem;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowState Run()
        {
            _codeBehindInstance = _codeBehindAdapter.InstantiateCodeBehind(_context.Components);

            try
            {
                _processorStateMachine.ReceiveTrigger(WorkflowProcessorTrigger.Run);

                var workflowState = _processorStateMachine.CurrentState.ToWorkflowState();
                return workflowState;
            }
            finally
            {
                _codeBehindInstance = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowState DispatchAndRun(IEnumerable<IWorkflowEvent> receivedEvents)
        {
            _codeBehindInstance = _codeBehindAdapter.InstantiateCodeBehind(_context.Components);

            try
            {
                _receivedEvents = receivedEvents.ToArray();
                _processorStateMachine.ReceiveTrigger(WorkflowProcessorTrigger.Resume);

                var workflowState = _processorStateMachine.CurrentState.ToWorkflowState();
                return workflowState;
            }
            finally
            {
                _codeBehindInstance = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid InstanceId 
        {
            get { return _instanceData.WorkflowInstanceId; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowState State
        {
            get { return _instanceData.WorkflowState; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime CreatedAtUtc
        {
            get { return _instanceData.CreatedAtUtc; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime StateChangedAtUtc
        {
            get { return _instanceData.UpdatedAtUtc; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type CodeBehindType
        {
            get { return _codeBehindAdapter.CodeBehindType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TimeSpan TotalTime
        {
            get { return _instanceData.TotalTime; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public TimeSpan TotalExecutionTime
        {
            get { return _instanceData.TotalExecutionTime; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TimeSpan TotalSuspensionTime
        {
            get { return _instanceData.TotalSuspensionTime; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int TotalSuspensionCount
        {
            get { return _instanceData.TotalSuspensionCount; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public WorkflowCodeBehindAdapter CodeBehindAdapter
        {
            get
            {
                return _codeBehindAdapter;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorInitializing(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            InvokeCodeBehind(e, () => {
                _codeBehindAdapter.OnInitialize(_codeBehindInstance, _instanceData, initializer: this);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorStarting(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            InvokeCodeBehind(e, () => {
                _codeBehindAdapter.OnStart(_codeBehindInstance);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorResuming(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            InvokeCodeBehind(e, () => {
                _codeBehindAdapter.OnResume(_codeBehindInstance, _instanceData);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorRunning(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            try
            {
                var preRunState = _instanceData.WorkflowState;
                SetWorkflowState(WorkflowState.Running);

                var processor = new WorkflowProcessor(_framework, this);
                _codeBehindAdapter.OnBuildWorkflow(_codeBehindInstance, processor);
                processor.EndBuildWorkflow();

                ProcessorResult result;

                if ( preRunState == WorkflowState.Suspended )
                {
                    processor.RestoreSnapshot(DeserializeProcessorSnapshot(_instanceData.ProcessorSnapshot));
                    result = processor.DispatchAndRun(_receivedEvents ?? new IWorkflowEvent[0]);
                }
                else
                {
                    result = processor.Run();
                }

                if ( result == ProcessorResult.Suspended )
                {
                    _instanceData.ProcessorSnapshot = SerializeProcessorSnapshot(processor.TakeSnapshot());
                }

                e.ReceiveFeedack(result == ProcessorResult.Completed ? WorkflowProcessorTrigger.Completed : WorkflowProcessorTrigger.Suspended);
            }
            catch ( Exception exc )
            {
                AddFailureException(exc);
                e.ReceiveFeedack(WorkflowProcessorTrigger.Failure);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorSuspending(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            InvokeCodeBehind(e, () => {
                _codeBehindAdapter.OnSuspend(_codeBehindInstance, _instanceData);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorSuspended(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            SetWorkflowState(WorkflowState.Suspended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorCompleting(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            InvokeCodeBehind(e, () => {
                _codeBehindAdapter.OnComplete(_codeBehindInstance);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorFailing(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            InvokeCodeBehind(e, () => {
                _codeBehindAdapter.OnFail(_codeBehindInstance, _failureException);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorFinalizing(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            InvokeCodeBehind(
                e, 
                () => {
                    _codeBehindAdapter.OnFinalize(_codeBehindInstance);
                },
                successFeedback: e.Trigger);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorCompletedTerminated(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            SetWorkflowState(WorkflowState.Completed);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnProcessorFailedTerminated(object sender, StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> e)
        {
            SetWorkflowState(WorkflowState.Failed);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InvokeCodeBehind(
            StateMachineFeedbackEventArgs<WorkflowProcessorState, WorkflowProcessorTrigger> eventArgs,
            Action action, 
            WorkflowProcessorTrigger successFeedback = WorkflowProcessorTrigger.Success)
        {
            try
            {
                action();
            }
            catch ( Exception e )
            {
                AddFailureException(e);
                eventArgs.ReceiveFeedack(WorkflowProcessorTrigger.Failure);
                return;
            }

            eventArgs.ReceiveFeedack(WorkflowProcessorTrigger.Success);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetWorkflowState(WorkflowState newState)
        {
            _instanceData.WorkflowState = newState;
            _instanceData.UpdatedAtUtc = _framework.UtcNow;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private byte[] SerializeProcessorSnapshot(WorkflowProcessorSnapshot snapshot)
        {
            using ( var stream = new MemoryStream() )
            {
                Serializer.Serialize<WorkflowProcessorSnapshot>(stream, snapshot);
                return stream.ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private WorkflowProcessorSnapshot DeserializeProcessorSnapshot(byte[] serializedSnapshot)
        {
            using ( var stream = new MemoryStream(serializedSnapshot) )
            {
                return Serializer.Deserialize<WorkflowProcessorSnapshot>(stream);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddFailureException(Exception exception)
        {
            if ( this._failureException == null )
            {
                this._failureException = exception;
            }
            else
            {
                this._failureException = new AggregateException(this._failureException, exception).Flatten();
            }
        }
    }
}
