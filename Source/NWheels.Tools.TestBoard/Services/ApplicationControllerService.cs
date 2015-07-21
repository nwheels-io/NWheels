using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Processing.Workflows;
using NWheels.Tools.TestBoard.Messages;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;

namespace NWheels.Tools.TestBoard.Services
{
    public interface IApplicationControllerService
    {
        bool CanLoad();
        bool CanStart();
        bool CanStop();
        bool CanUnload();
        Task LoadAsync(string bootConfigFilePath);
        Task StartAsync();
        Task StopAsync();
        Task UnloadAsync();
        BootConfiguration BootConfig { get; }
        NodeHost NodeHost { get; }
        ApplicationState CurrentState { get; }
        event EventHandler CurrentStateChanged;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [Export(typeof(IApplicationControllerService))]
    public class ApplicationControllerService : IApplicationControllerService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationComponentInjector[] _componentInjectors;
        private readonly TransientStateMachine<ApplicationState, ApplicationTrigger> _stateMachine;
        private string _bootConfigFilePath;
        private BootConfiguration _bootConfig;
        private NodeHost _nodeHost;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationControllerService(
            IEventAggregator eventAggregator, 
            [ImportMany] IEnumerable<IApplicationComponentInjector> componentInjectors)
        {
            _eventAggregator = eventAggregator;
            _componentInjectors = componentInjectors.ToArray();
            
            _stateMachine = new TransientStateMachine<ApplicationState, ApplicationTrigger>(
                new StateMachineCodeBehind(this), 
                (TransientStateMachine<ApplicationState, ApplicationTrigger>.ILogger)new StateMachineLogger());
            _stateMachine.CurrentStateChanged += OnCurrentStateChanged;

            _bootConfigFilePath = null;
            _bootConfig = null;
            _nodeHost = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task LoadAsync(string bootConfigFilePath)
        {
            return Task.Run(() => _stateMachine.ReceiveTrigger(ApplicationTrigger.LoadRequested, context: bootConfigFilePath));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task StartAsync()
        {
            return Task.Run(() => _stateMachine.ReceiveTrigger(ApplicationTrigger.StartRequested));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task StopAsync()
        {
            return Task.Run(() => _stateMachine.ReceiveTrigger(ApplicationTrigger.StopRequested));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task UnloadAsync()
        {
            return Task.Run(() => _stateMachine.ReceiveTrigger(ApplicationTrigger.UnloadRequested));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CanUnload()
        {
            return (_stateMachine.CurrentState == ApplicationState.Stopped);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CanLoad()
        {
            return _stateMachine.CurrentState.IsIn(ApplicationState.NotLoaded, ApplicationState.Stopped);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CanStart()
        {
            return (_stateMachine.CurrentState == ApplicationState.Stopped);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CanStop()
        {
            return (_stateMachine.CurrentState == ApplicationState.Running);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BootConfiguration BootConfig
        {
            get
            {
                return _bootConfig;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHost NodeHost
        {
            get
            {
                return _nodeHost;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationState CurrentState
        {
            get
            {
                return _stateMachine.CurrentState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler CurrentStateChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Load(string bootConfigFilePath)
        {
            _bootConfigFilePath = bootConfigFilePath;

            try
            {
                _bootConfig = BootConfiguration.LoadFromFile(bootConfigFilePath);
                _bootConfig.Validate();
                
                _eventAggregator.Publish(
                    new Messages.AppLoadedMessage(bootConfigFilePath, _bootConfig), 
                    action => action());
            }
            catch
            {
                _bootConfigFilePath = null;
                _bootConfig = null;

                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Start()
        {
            _nodeHost = new NodeHost(_bootConfig);

            try
            {
                _nodeHost.LoadAndActivate();
            }
            catch
            {
                _nodeHost = null;
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Stop()
        {
            try
            {
                _nodeHost.DeactivateAndUnload();
            }
            finally
            {
                _nodeHost = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Unload()
        {
            var appUnloadedMessage = new Messages.AppUnloadedMessage(_bootConfigFilePath, _bootConfig);

            _bootConfig = null;
            _bootConfigFilePath = null;

            _eventAggregator.Publish(appUnloadedMessage, action => action());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void OnCurrentStateChanged(object sender, EventArgs e)
        {
            if ( CurrentStateChanged != null )
            {
                CurrentStateChanged(this, EventArgs.Empty);
            }

            _eventAggregator.Publish(new AppControllerStateChangedMessage(_stateMachine.CurrentState), action => action());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum ApplicationTrigger
        {
            Success,
            Failure,
            LoadRequested,
            StartRequested,
            StopRequested,
            UnloadRequested
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineCodeBehind : IStateMachineCodeBehind<ApplicationState, ApplicationTrigger>
        {
            private readonly ApplicationControllerService _ownerHost;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateMachineCodeBehind(ApplicationControllerService ownerHost)
            {
                _ownerHost = ownerHost;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<ApplicationState, ApplicationTrigger> machine)
            {
                machine.State(ApplicationState.NotLoaded)
                    .SetAsInitial()
                    .OnTrigger(ApplicationTrigger.LoadRequested).TransitionTo(ApplicationState.Loading);

                machine.State(ApplicationState.Loading)
                    .OnEntered(OnLoadingEntered)
                    .OnTrigger(ApplicationTrigger.Success).TransitionTo(ApplicationState.Stopped)
                    .OnTrigger(ApplicationTrigger.Failure).TransitionTo(ApplicationState.NotLoaded);

                machine.State(ApplicationState.Stopped)
                    .OnTrigger(ApplicationTrigger.LoadRequested).TransitionTo(ApplicationState.Loading, OnTransitioningFromStoppedToLoading)
                    .OnTrigger(ApplicationTrigger.StartRequested).TransitionTo(ApplicationState.Starting)
                    .OnTrigger(ApplicationTrigger.UnloadRequested).TransitionTo(ApplicationState.Unloading);

                machine.State(ApplicationState.Unloading)
                    .OnEntered(OnUnloadingEntered)
                    .OnTrigger(ApplicationTrigger.Success).TransitionTo(ApplicationState.NotLoaded)
                    .OnTrigger(ApplicationTrigger.Failure).TransitionTo(ApplicationState.NotLoaded);

                machine.State(ApplicationState.Starting)
                    .OnEntered(OnStartingEntered)
                    .OnTrigger(ApplicationTrigger.Success).TransitionTo(ApplicationState.Running)
                    .OnTrigger(ApplicationTrigger.Failure).TransitionTo(ApplicationState.Stopped);

                machine.State(ApplicationState.Running)
                    .OnTrigger(ApplicationTrigger.StopRequested).TransitionTo(ApplicationState.Stopping);

                machine.State(ApplicationState.Stopping)
                    .OnEntered(OnStoppingEntered)
                    .OnTrigger(ApplicationTrigger.Success).TransitionTo(ApplicationState.Stopped)
                    .OnTrigger(ApplicationTrigger.Failure).TransitionTo(ApplicationState.Stopped);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnTransitioningFromStoppedToLoading(object sender, StateMachineEventArgs<ApplicationState, ApplicationTrigger> e)
            {
                _ownerHost.Unload();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void OnLoadingEntered(object sender, StateMachineFeedbackEventArgs<ApplicationState, ApplicationTrigger> e)
            {
                try
                {
                    _ownerHost.Load(bootConfigFilePath: (string)e.Context);
                    e.ReceiveFeedback(ApplicationTrigger.Success);
                }
                catch
                {
                    e.ReceiveFeedback(ApplicationTrigger.Failure);
                    throw;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnStartingEntered(object sender, StateMachineFeedbackEventArgs<ApplicationState, ApplicationTrigger> e)
            {
                try
                {
                    _ownerHost.Start();
                    e.ReceiveFeedback(ApplicationTrigger.Success);
                }
                catch
                {
                    e.ReceiveFeedback(ApplicationTrigger.Failure);
                    throw;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnStoppingEntered(object sender, StateMachineFeedbackEventArgs<ApplicationState, ApplicationTrigger> e)
            {
                try
                {
                    _ownerHost.Stop();
                }
                finally
                {
                    e.ReceiveFeedback(ApplicationTrigger.Success);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void OnUnloadingEntered(object sender, StateMachineFeedbackEventArgs<ApplicationState, ApplicationTrigger> e)
            {
                try
                {
                    _ownerHost.Unload();
                }
                finally
                {
                    e.ReceiveFeedback(ApplicationTrigger.Success);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineLogger : TransientStateMachine<ApplicationState, ApplicationTrigger>.ILogger
        {
            public Exceptions.CodeBehindErrorException InitialStateNotSet(Type codeBehind)
            {
                return new CodeBehindErrorException("InitialStateNotSet");
            }
            public Exceptions.CodeBehindErrorException StateAlreadyDefined(Type codeBehind, ApplicationState state)
            {
                return new CodeBehindErrorException("StateAlreadyDefined");
            }
            public Exceptions.CodeBehindErrorException InitialStateAlreadyDefined(Type codeBehind, ApplicationState initialState, ApplicationState attemptedState)
            {
                return new CodeBehindErrorException("InitialStateAlreadyDefined");
            }
            public Exceptions.CodeBehindErrorException TransitionAlreadyDefined(Type codeBehind, ApplicationState state, ApplicationTrigger trigger)
            {
                return new CodeBehindErrorException("TransitionAlreadyDefined");
            }
            public Exceptions.CodeBehindErrorException TransitionNotDefined(Type codeBehind, ApplicationState state, ApplicationTrigger trigger)
            {
                return new CodeBehindErrorException("TransitionNotDefined");
            }
        }
    }
}
