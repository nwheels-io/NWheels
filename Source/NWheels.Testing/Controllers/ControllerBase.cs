using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Processing.Workflows;
using NWheels.Hosting.Core;
using NWheels.Logging.Core;
using Autofac;

namespace NWheels.Testing.Controllers
{
    public abstract class ControllerBase
    {
        private readonly ControllerBase _parentController;
        private readonly IPlainLog _log;
        private readonly List<Exception> _errors;
        private readonly TransientStateMachine<NodeState, NodeTrigger> _stateMachine;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ControllerBase(ControllerBase parentController, IPlainLog log)
        {
            _log = log ?? parentController.Log;
            _parentController = parentController;

            TransientStateMachine<NodeState, NodeTrigger>.ILogger stateLogger = new StateMachineLogger();

            _stateMachine = new TransientStateMachine<NodeState, NodeTrigger>(new StateMchineCodeBehind(this), stateLogger);
            _stateMachine.CurrentStateChanged += OnCurrentStateChanged;
            _errors = new List<Exception>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ControllerBase(IPlainLog log) 
            : this(parentController: null, log: log)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ControllerBase(ControllerBase parentController)
            : this(parentController, log: null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool CanLoad()
        {
            return _stateMachine.CurrentState.IsIn(NodeState.Down);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool CanActivate()
        {
            return _stateMachine.CurrentState.IsIn(NodeState.Standby);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool CanDeactivate()
        {
            return _stateMachine.CurrentState.IsIn(NodeState.Active);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool CanUnload()
        {
            return _stateMachine.CurrentState.IsIn(NodeState.Standby);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
            PerformRequestTrigger(NodeTrigger.Load);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activate()
        {
            PerformRequestTrigger(NodeTrigger.Activate);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadAndActivate()
        {
            Load();

            if ( !_errors.Any() )
            {
                Activate();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeactivateAndUnload()
        {
            try
            {
                Deactivate();
            }
            finally
            {
                Unload();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivate()
        {
            PerformRequestTrigger(NodeTrigger.Deactivate);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unload()
        {
            PerformRequestTrigger(NodeTrigger.Unload);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ControllerBase[] GetSubControllers()
        {
            return new ControllerBase[0];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract IEnumerable<ILogConnection> CreateLogConnections();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string DisplayName { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string QualifiedName
        {
            get
            {
                var parentPrefix = (_parentController != null ? _parentController.QualifiedName + "::" : "");
                
                var qualifiedName = string.Format(
                    "{0}{1}[{2}]",
                    parentPrefix,
                    this.GetType().Name.TrimTail("Controller").SplitPascalCase(), 
                    this.DisplayName);

                return qualifiedName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ControllerBase ParentController
        {
            get { return _parentController; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeState CurrentState
        {
            get
            {
                return _stateMachine.CurrentState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<Exception> Errors
        {
            get
            {
                return _errors;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler<ComponentInjectionEventArgs> InjectingComponents;
        public event EventHandler<ControllerStateEventArgs> CurrentStateChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnLoad();
        protected abstract void OnActivate();
        protected abstract void OnDeactivate();
        protected abstract void OnUnload();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnInjectingComponents(Autofac.ContainerBuilder containerBuilder)
        {
            if ( _parentController != null )
            {
                _parentController.OnInjectingComponents(containerBuilder);
            }

            if ( InjectingComponents != null )
            {
                InjectingComponents(this, new ComponentInjectionEventArgs(containerBuilder));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnCurrentStateChanged(object sender, EventArgs e)
        {
            Log.Debug("Controller {0} -> {1}", this.QualifiedName, this.CurrentState);

            if ( CurrentStateChanged != null )
            {
                CurrentStateChanged(this, new ControllerStateEventArgs(this, _stateMachine.CurrentState));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void AddError(Exception error)
        {
            _errors.Add(error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IPlainLog Log
        {
            get
            {
                return _log;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PerformRequestTrigger(NodeTrigger trigger)
        {
            _errors.Clear();
            _log.Info("{0} {1}.", trigger.ToString().AddEnglishVerbIngSuffix(), this.QualifiedName);

            try
            {
                _stateMachine.ReceiveTrigger(trigger);

                if ( _errors.Count > 0 )
                {
                    LogErrors(trigger);
                    throw new ControllerRequestFailedException(this, trigger);
                }
            }
            catch ( ControllerRequestFailedException )
            {
                throw;
            }
            catch ( Exception e )
            {
                _errors.Add(e);

                _log.Error(
                    "------------- EXCEPTIONS -------------\r\nFAILED TO {0} {1}: {2}\r\n---------- END OF EXCEPTIONS ----------", 
                    trigger.ToString().ToUpper(), this.QualifiedName, e.ToString());

                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LogErrors(NodeTrigger trigger)
        {
            var errorText = new StringBuilder();
            
            errorText.AppendFormat(
                "{0} error(s) occurred while {1} {2}: ", 
                _errors.Count, trigger.ToString().AddEnglishVerbIngSuffix().ToLower(), this.QualifiedName);

            errorText.AppendLine();
            errorText.AppendLine("------------- ERRORS ---------------");

            foreach ( var error in _errors )
            {
                if ( error is ControllerRequestFailedException )
                {
                    errorText.AppendLine(error.Message);
                }
                else
                {
                    errorText.AppendLine(error.ToString());
                }
            }

            errorText.Append("---------- END OF ERRORS -----------");

            _log.Error(errorText.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMchineCodeBehind : IStateMachineCodeBehind<NodeState, NodeTrigger>
        {
            private readonly ControllerBase _ownerController;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateMchineCodeBehind(ControllerBase ownerController)
            {
                _ownerController = ownerController;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<NodeState, NodeTrigger> machine)
            {
                machine.State(NodeState.Down)
                    .SetAsInitial()
                    .OnTrigger(NodeTrigger.Load).TransitionTo(NodeState.Loading);

                machine.State(NodeState.Loading)
                    .OnTrigger(NodeTrigger.LoadSuccess).TransitionTo(NodeState.Standby)
                    .OnTrigger(NodeTrigger.LoadFailure).TransitionTo(NodeState.Down)
                    .OnEntered(LoadingEntered);

                machine.State(NodeState.Standby)
                    .OnTrigger(NodeTrigger.Activate).TransitionTo(NodeState.Activating)
                    .OnTrigger(NodeTrigger.Unload).TransitionTo(NodeState.Unloading);

                machine.State(NodeState.Activating)
                    .OnTrigger(NodeTrigger.ActivateSuccess).TransitionTo(NodeState.Active)
                    .OnTrigger(NodeTrigger.ActivateFailure).TransitionTo(NodeState.Standby)
                    .OnEntered(ActivatingEntered);

                machine.State(NodeState.Active)
                    .OnTrigger(NodeTrigger.Deactivate).TransitionTo(NodeState.Deactivating);

                machine.State(NodeState.Deactivating)
                    .OnTrigger(NodeTrigger.DeactivateDone).TransitionTo(NodeState.Standby)
                    .OnEntered(DeactivatingEntered);

                machine.State(NodeState.Unloading)
                    .OnTrigger(NodeTrigger.UnloadDone).TransitionTo(NodeState.Down)
                    .OnEntered(UnloadingEntered);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                PerformStateAction(e, _ownerController.OnLoad, NodeTrigger.LoadSuccess, NodeTrigger.LoadFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                PerformStateAction(e, _ownerController.OnActivate, NodeTrigger.ActivateSuccess, NodeTrigger.ActivateFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                PerformStateAction(e, _ownerController.OnDeactivate, NodeTrigger.DeactivateDone, NodeTrigger.DeactivateDone);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                PerformStateAction(e, _ownerController.OnUnload, NodeTrigger.UnloadDone, NodeTrigger.UnloadDone);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PerformStateAction(
                StateMachineFeedbackEventArgs<NodeState, NodeTrigger> args, 
                Action stateAction,
                NodeTrigger successFeedback,
                NodeTrigger failureFeedback)
            {
                try
                {
                    stateAction();

                    if ( _ownerController.Errors.Count == 0 )
                    {
                        args.ReceiveFeedback(successFeedback);
                    }
                    else
                    {
                        _ownerController.AddError(new Exception("One or more errors occurred in controller: " + _ownerController.QualifiedName));
                        args.ReceiveFeedback(failureFeedback);
                    }
                }
                catch ( Exception error )
                {
                    _ownerController.AddError(error);
                    args.ReceiveFeedback(failureFeedback);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineLogger : TransientStateMachine<NodeState, NodeTrigger>.ILogger
        {
            public Exceptions.CodeBehindErrorException InitialStateNotSet(Type codeBehind)
            {
                return new CodeBehindErrorException("InitialStateNotSet");
            }
            public Exceptions.CodeBehindErrorException StateAlreadyDefined(Type codeBehind, NodeState state)
            {
                return new CodeBehindErrorException("StateAlreadyDefined");
            }
            public Exceptions.CodeBehindErrorException InitialStateAlreadyDefined(Type codeBehind, NodeState initialState, NodeState attemptedState)
            {
                return new CodeBehindErrorException("InitialStateAlreadyDefined");
            }
            public Exceptions.CodeBehindErrorException TransitionAlreadyDefined(Type codeBehind, NodeState state, NodeTrigger trigger)
            {
                return new CodeBehindErrorException("TransitionAlreadyDefined");
            }
            public Exceptions.CodeBehindErrorException TransitionNotDefined(Type codeBehind, NodeState state, NodeTrigger trigger)
            {
                return new CodeBehindErrorException("TransitionNotDefined");
            }
        }
    }
}
