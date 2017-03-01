using NWheels.Injection;
using NWheels.Orchestration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace NWheels.Microservices
{
    public class MicroserviceHost
    {
        private readonly BootConfiguration _bootConfig;
        private readonly string _modulesPath;
        private readonly List<ILifecycleListenerComponent> _lifecycleComponents;
        private readonly RevertableSequence _configureSequence;
        private readonly RevertableSequence _loadSequence;
        private readonly RevertableSequence _activateSequence;
        private int _initializationCount = 0;
        private IContainer _container;
        private IMicroserviceHostLogger _logger;        
        private TransientStateMachine<MicroserviceState, MicroserviceTrigger> _stateMachine;        

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHost(BootConfiguration bootConfig, string modulesPath)
        {
            _bootConfig = bootConfig;
            _modulesPath = modulesPath;
            _configureSequence = new RevertableSequence(new ConfigureSequenceCodeBehind(this));
            _loadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
            _activateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
            _lifecycleComponents = new List<ILifecycleListenerComponent>();
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler StateChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure()
        {
            InitializeBeforeConfigure();

            using (_logger.NodeConfiguring())
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
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
            using (_logger.NodeLoading())
            {
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
            InitializeBeforeConfigure();

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

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IMicroserviceHostLogger Logger
        {
            get
            {
                return _logger;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private BootConfiguration BootConfig
        {
            get
            {
                return _bootConfig;
            }
        }

        private void CreateContainer(IContainerBuilder builder)
        {
            //TODO: validate _container == null
            _container = builder.CreateContainer();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteConfigurePhase()
        {
            return ExecutePhase(_configureSequence.Perform, _logger.NodeConfigureError);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteLoadPhase()
        {
            return ExecutePhase(_loadSequence.Perform, _logger.NodeLoadError);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteActivatePhase()
        {
            return ExecutePhase(_activateSequence.Perform, _logger.NodeActivationError);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteDeactivatePhase()
        {
            return ExecutePhase(_activateSequence.Revert, _logger.NodeDeactivationError);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteUnloadPhase()
        {
            return ExecutePhase(
                () => {
                    _loadSequence.Revert();
                    _configureSequence.Revert();
                }, 
                _logger.NodeUnloadError);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecutePhase(Action phaseAction, Action<Exception> loggerAction)
        {
            try
            {
                phaseAction();
                return true;
            }
            catch(Exception e)
            {
                loggerAction(e);
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeBeforeConfigure()
        {
            if (Interlocked.Increment(ref _initializationCount) > 1)
            {
                return;
            }

            //_container = BuildBaseContainer(_registerHostComponents);

            _stateMachine = new TransientStateMachine<MicroserviceState, MicroserviceTrigger>(
                new StateMachineCodeBehind(this),
                new Mocks.TransientStateMachineLoggerMock<MicroserviceState, MicroserviceTrigger>());
                //_container.Resolve<TransientStateMachine<MicroserviceState, MicroserviceTrigger>.ILogger>());
            _stateMachine.CurrentStateChanged += OnStateChanged;
            
            _logger = new Mocks.MicroserviceHostLoggerMock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FinalizeAfterUnload()
        {
            if (Interlocked.Decrement(ref _initializationCount) > 0)
            {
                return;
            }

            _container.Dispose();
            _container = null;
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
                var success = _owner.ExecuteConfigurePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _owner.ExecuteLoadPhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _owner.ExecuteActivatePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _owner.ExecuteDeactivatePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                _owner.ExecuteUnloadPhase();
                e.ReceiveFeedback(MicroserviceTrigger.Done);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class MicroserviceLifecycleSequenceBase
        {
            private readonly MicroserviceHost _ownerHost;
            private IDisposable _systemSession;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected MicroserviceLifecycleSequenceBase(MicroserviceHost ownerHost)
            {
                _ownerHost = ownerHost;
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

            protected MicroserviceHost OwnerHost
            {
                get { return _ownerHost; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ConfigureSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            private readonly IMicroserviceHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ConfigureSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
                _logger = OwnerHost.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(RegisterFeatureLoaders);
            }

            private void RegisterFeatureLoaders()
            {
                IContainerBuilder builder = null;
                /*var type = asm.GetType("MyClassLib.SampleClasses.Sample");
                dynamic obj = Activator.CreateInstance(type);*/

                var featureLoaders = new List<Type>();

                foreach (var file in Directory.GetFiles(OwnerHost.BootConfig.ModulesDirectory, "*.dll"))
                {
                    var assemblyLoadContext = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                    //var assembly = Assembly.Load(assemblyLoadContext.GetName());
                    try
                    {
                        foreach (var type in assemblyLoadContext.GetTypes())
                        {
                            foreach (var implementedInterface in type.GetTypeInfo().ImplementedInterfaces)
                            {
                                if (implementedInterface == typeof(IFeatureLoader))
                                {
                                    featureLoaders.Add(type);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var info = ex.ToString();
                    }
                }

                OwnerHost.CreateContainer(builder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            private readonly IMicroserviceHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoadSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
                _logger = OwnerHost.Logger;
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

            public ActivateSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
                _logger = OwnerHost.Logger;
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
