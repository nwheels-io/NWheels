using NWheels.Injection;
using NWheels.Orchestration;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NWheels.Microservices
{
    public class MicroserviceHost
    {
        private readonly BootConfiguration _bootConfig;
        private int _initializationCount = 0;
        private IContainer _baseContainer;
        private IMicroserviceHostLogger _logger;        
        private TransientStateMachine<MicroserviceState, MicroserviceTrigger> _stateMachine;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHost(BootConfiguration bootConfig)
        {
            _bootConfig = bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler StateChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
            InitializeBeforeLoad();

            using (_logger.NodeLoading())
            {
                try
                {
                    _stateMachine.ReceiveTrigger(MicroserviceTrigger.Configure);
                }
                catch (Exception e)
                {
                    _logger.NodeHasFailedToConfigure(e);
                    throw;
                }

                if (_stateMachine.CurrentState != MicroserviceState.Configured)
                {
                    throw _logger.NodeHasFailedToConfigure();
                }

                try
                {
                    _stateMachine.ReceiveTrigger(MicroserviceTrigger.Load);
                }
                catch (Exception e)
                {
                    _logger.NodeHasFailedToLoad(e);
                    throw;
                }

                if (_stateMachine.CurrentState != MicroserviceState.Standby)
                {
                    throw _logger.NodeHasFailedToLoad();
                }

                _logger.NodeSuccessfullyLoaded();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activate()
        {
            using (_logger.NodeActivating())
            {
                try
                {
                    _stateMachine.ReceiveTrigger(MicroserviceTrigger.Activate);
                }
                catch (Exception e)
                {
                    _logger.NodeHasFailedToActivate(e);
                    throw;
                }

                if (_stateMachine.CurrentState != MicroserviceState.Active)
                {
                    throw _logger.NodeHasFailedToActivate();
                }

                _logger.NodeSuccessfullyActivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadAndActivate()
        {
            InitializeBeforeLoad();

            using (_logger.NodeStartingUp(
                /*_bootConfig.ApplicationName,
                _bootConfig.EnvironmentType,
                _bootConfig.EnvironmentName,
                _bootConfig.NodeName,
                _bootConfig.InstanceId*/))
            {
                Load();
                Activate();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivate()
        {
            using (_logger.NodeDeactivating())
            {
                _stateMachine.ReceiveTrigger(MicroserviceTrigger.Deactivate);

                if (_stateMachine.CurrentState != MicroserviceState.Standby)
                {
                    throw _logger.NodeHasFailedToDeactivate();
                }

                _logger.NodeDeactivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeactivateAndUnload()
        {
            using (_logger.NodeShuttingDown())
            {
                if (_stateMachine.CurrentState == MicroserviceState.Active)
                {
                    Deactivate();
                }

                if (_stateMachine.CurrentState == MicroserviceState.Standby)
                {
                    Unload();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unload()
        {
            using (_logger.NodeUnloading())
            {
                _stateMachine.ReceiveTrigger(MicroserviceTrigger.Unload);

                if (_stateMachine.CurrentState != MicroserviceState.Down)
                {
                    throw _logger.NodeHasFailedToUnload();
                }

                _logger.NodeUnloaded();
            }

            FinalizeAfterUnload();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MicroserviceLifetime CreateNodeLifetime()
        {
            return new MicroserviceLifetime(
                _bootConfig,
                _baseContainer,
                _logger);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeBeforeLoad()
        {
            if (Interlocked.Increment(ref _initializationCount) > 1)
            {
                return;
            }

            //_baseContainer = BuildBaseContainer(_registerHostComponents);

            _stateMachine = new TransientStateMachine<MicroserviceState, MicroserviceTrigger>(
                new StateMachineCodeBehind(this),
                _baseContainer.Resolve<TransientStateMachine<MicroserviceState, MicroserviceTrigger>.ILogger>());
            _stateMachine.CurrentStateChanged += OnStateChanged;

            _logger = _baseContainer.Resolve<IMicroserviceHostLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FinalizeAfterUnload()
        {
            if (Interlocked.Decrement(ref _initializationCount) > 0)
            {
                return;
            }

            _baseContainer.Dispose();
            _baseContainer = null;
            _stateMachine = null;
            _logger = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (StateChanged != null)
            {
                StateChanged(this, e);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineCodeBehind : IStateMachineCodeBehind<MicroserviceState, MicroserviceTrigger>
        {
            private readonly MicroserviceHost _owner;
            private MicroserviceLifetime _lifetime = null;            

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateMachineCodeBehind(MicroserviceHost owner)
            {
                _owner = owner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<MicroserviceState, MicroserviceTrigger> machine)
            {
                machine.State(MicroserviceState.Down)
                    .SetAsInitial()
                    .OnTrigger(MicroserviceTrigger.Configure).TransitionTo(MicroserviceState.Configuring);

                machine.State(MicroserviceState.Configuring)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Down)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Configured)
                    .OnEntered(ConfiguringEntered);

                machine.State(MicroserviceState.Configured)
                    .OnTrigger(MicroserviceTrigger.Unload).TransitionTo(MicroserviceState.Down)
                    .OnTrigger(MicroserviceTrigger.Load).TransitionTo(MicroserviceState.Loading);

                machine.State(MicroserviceState.Loading)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Down)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Standby)
                    .OnEntered(LoadingEntered);

                machine.State(MicroserviceState.Standby)
                    .OnTrigger(MicroserviceTrigger.Unload).TransitionTo(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.Activate).TransitionTo(MicroserviceState.Activating);

                machine.State(MicroserviceState.Activating)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Active)
                    .OnEntered(ActivatingEntered);

                machine.State(MicroserviceState.Active)
                    .OnTrigger(MicroserviceTrigger.Deactivate).TransitionTo(MicroserviceState.Deactivating);

                machine.State(MicroserviceState.Deactivating)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Standby)
                    .OnEntered(DeactivatingEntered);

                machine.State(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.Done).TransitionTo(MicroserviceState.Down)
                    .OnEntered(UnloadingEntered);
            }            

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ConfiguringEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                _lifetime = _owner.CreateNodeLifetime();
                var success = _lifetime.ExecuteConfigurePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _lifetime.ExecuteLoadPhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _lifetime.ExecuteActivatePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _lifetime.ExecuteDeactivatePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                _lifetime.ExecuteUnloadPhase();
                e.ReceiveFeedback(MicroserviceTrigger.Done);
                _lifetime = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class MicroserviceLifetime //: IDisposable, ITenantIdentificationStrategy
        {
            private readonly BootConfiguration _nodeConfig;
            private readonly IMicroserviceHostLogger _logger;
            private readonly IContainer _lifetimeContainer;
            private readonly List<ILifecycleListenerComponent> _lifecycleComponents;
            private readonly RevertableSequence _configureSequence;
            private readonly RevertableSequence _loadSequence;
            private readonly RevertableSequence _activateSequence;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MicroserviceLifetime(
                BootConfiguration microserviceConfig,
                IContainer baseContainer,
                IMicroserviceHostLogger logger)
            {
                _nodeConfig = microserviceConfig;
                _logger = logger;
                _lifetimeContainer = baseContainer;
                _configureSequence = new RevertableSequence(new ConfigureSequenceCodeBehind(this));
                _loadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
                _activateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
                _lifecycleComponents = new List<ILifecycleListenerComponent>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExecuteConfigurePhase()
            {
                try
                {
                    _configureSequence.Perform();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.NodeConfigureError(e);
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExecuteLoadPhase()
            {
                try
                {
                    _loadSequence.Perform();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.NodeLoadError(e);
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExecuteActivatePhase()
            {
                try
                {
                    _activateSequence.Perform();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.NodeActivationError(e);
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExecuteDeactivatePhase()//???
            {
                try
                {
                    _activateSequence.Revert();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.NodeDeactivationError(e);
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExecuteUnloadPhase()
            {
                try
                {
                    _loadSequence.Revert();
                    _configureSequence.Revert();//???
                }
                catch (Exception e)
                {
                    _logger.NodeUnloadError(e);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BootConfiguration NodeConfig
            {
                get
                {
                    return _nodeConfig;
                }
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<ILifecycleListenerComponent> LifecycleComponents
            {
                get
                {
                    return _lifecycleComponents;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IMicroserviceHostLogger Logger
            {
                get
                {
                    return _logger;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class MicroserviceLifecycleSequenceBase
        {
            private readonly MicroserviceLifetime _ownerLifetime;
            private IDisposable _systemSession;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected MicroserviceLifecycleSequenceBase(MicroserviceLifetime ownerLifetime)
            {
                _ownerLifetime = ownerLifetime;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void JoinSystemSession()
            {
                //_systemSession = _ownerLifetime.LifetimeContainer.Resolve<ISessionManager>().JoinGlobalSystem();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void LeaveSystemSession()
            {
                _systemSession.Dispose();
                _systemSession = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected MicroserviceLifetime OwnerLifetime
            {
                get { return _ownerLifetime; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ConfigureSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            private readonly IMicroserviceHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ConfigureSequenceCodeBehind(MicroserviceLifetime ownerLifetime)
                : base(ownerLifetime)
            {
                _logger = OwnerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            private readonly IMicroserviceHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoadSequenceCodeBehind(MicroserviceLifetime ownerLifetime)
                : base(ownerLifetime)
            {
                _logger = OwnerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                /*sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                sequence.Once().OnRevert(SaveDynamicModuleToAssembly);
                sequence.Once().OnRevert(WriteEffectiveMetadataJson);
                sequence.Once().OnPerform(LoadConfiguration);
                sequence.Once().OnPerform(WriteEffectiveConfigurationXml);
                sequence.Once().OnPerform(PreInitializeComponents);
                sequence.Once().OnPerform(InitializeDataAccessComponents);
                sequence.Once().OnPerform(FindLifecycleComponents);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeConfigured);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoading).OnRevert(CallComponentNodeUnloaded);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentLoad).OnRevert(CallComponentUnload);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoaded).OnRevert(CallComponentNodeUnloading);
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);*/
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActivateSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            private readonly IMicroserviceHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActivateSequenceCodeBehind(MicroserviceLifetime ownerLifetime)
                : base(ownerLifetime)
            {
                _logger = OwnerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                /*sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivating).OnRevert(CallComponentNodeDeactivated);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentActivate).OnRevert(CallComponentDeactivate);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivated).OnRevert(CallComponentNodeDeactivating);
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);*/
            }
        }
    }
}
