using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Hapil;
using NWheels.Conventions;
using NWheels.Core.Conventions;
using NWheels.Core.Logging;
using NWheels.Core.Processing;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Core.Hosting
{
    public class NodeHost : INodeHost
    {
        private readonly NodeHostConfig _nodeHostConfig;
        private readonly DynamicModule _dynamicModule;
        private readonly IContainer _baseContainer;
        private readonly INodeHostLogger _logger;
        private readonly StateMachine<NodeState, NodeTrigger> _stateMachine;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHost(NodeHostConfig config, Action<ContainerBuilder> registerHostComponents = null)
        {
            _nodeHostConfig = config;
            
            _dynamicModule = new DynamicModule(
                simpleName: "NWheels.RunTimeTypes", 
                allowSave: true, 
                saveDirectory: PathUtility.LocalBinPath());

            _baseContainer = BuildBaseContainer(registerHostComponents);
            
            _stateMachine = new StateMachine<NodeState, NodeTrigger>(
                new StateMachineCodeBehind(this), 
                _baseContainer.Resolve<Auto<StateMachine<NodeState, NodeTrigger>.ILogger>>());

            _logger = _baseContainer.ResolveAuto<INodeHostLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
            using ( _logger.NodeLoading() )
            {
                _stateMachine.ReceiveTrigger(NodeTrigger.Load);

                if ( _stateMachine.CurrentState != NodeState.Standby )
                {
                    throw _logger.NodeHasFailedToLoad();
                }

                _logger.NodeSuccessfullyLoaded();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activate()
        {
            using ( _logger.NodeActivating() )
            {
                _stateMachine.ReceiveTrigger(NodeTrigger.Activate);

                if ( _stateMachine.CurrentState != NodeState.Active )
                {
                    throw _logger.NodeHasFailedToActivate();
                }

                _logger.NodeSuccessfullyActivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadAndActivate()
        {
            using ( _logger.NodeStartingUp() )
            {
                Load();
                Activate();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivate()
        {
            using ( _logger.NodeDeactivating() )
            {
                _stateMachine.ReceiveTrigger(NodeTrigger.Deactivate);

                if ( _stateMachine.CurrentState != NodeState.Standby )
                {
                    throw _logger.NodeHasFailedToDeactivate();
                }

                _logger.NodeDeactivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unload()
        {
            using ( _logger.NodeUnloading() )
            {
                _stateMachine.ReceiveTrigger(NodeTrigger.Unload);

                if ( _stateMachine.CurrentState != NodeState.Down )
                {
                    throw _logger.NodeHasFailedToUnload();
                }

                _logger.NodeUnloaded();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeactivateAndUnload()
        {
            using ( _logger.NodeShuttingDown() )
            {
                Deactivate();
                Unload();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ApplicationName
        {
            get
            {
                return _nodeHostConfig.ApplicationName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NodeName
        {
            get
            {
                return _nodeHostConfig.NodeName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeState State
        {
            get
            {
                return _stateMachine.CurrentState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IContainer BuildBaseContainer(Action<ContainerBuilder> registerHostComponents)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance<NodeHost>(this);
            builder.RegisterInstance<DynamicModule>(_dynamicModule);
            builder.RegisterGeneric(typeof(Auto<>)).SingleInstance();
            builder.RegisterType<UniversalThreadLogAnchor>().As<IThreadLogAnchor>().SingleInstance();
            builder.RegisterType<ThreadRegistry>().As<IThreadRegistry>().SingleInstance();
            builder.RegisterType<ThreadLogAppender>().As<IThreadLogAppender>().SingleInstance();
            builder.RegisterType<RealFramework>().As<IFramework>().SingleInstance();
            builder.RegisterType<LoggerObjectFactory>().As<IAutoObjectFactory>().SingleInstance();

            if ( registerHostComponents != null )
            {
                registerHostComponents(builder);
            }

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private NodeLifetime CreateNodeLifetime()
        {
            return new NodeLifetime(_nodeHostConfig, _baseContainer, _logger);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineCodeBehind : IStateMachineCodeBehind<NodeState, NodeTrigger>
        {
            private readonly NodeHost _owner;
            private NodeLifetime _lifetime = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateMachineCodeBehind(NodeHost owner)
            {
                _owner = owner;
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
                _lifetime = _owner.CreateNodeLifetime();
                var success = _lifetime.ExecuteLoadPhase();
                e.ReceiveFeedack(success ? NodeTrigger.LoadSuccess : NodeTrigger.LoadFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                var success = _lifetime.ExecuteActivatePhase();
                e.ReceiveFeedack(success ? NodeTrigger.ActivateSuccess : NodeTrigger.ActivateFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _lifetime.ExecuteDeactivatePhase();
                e.ReceiveFeedack(NodeTrigger.DeactivateDone);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _lifetime.ExecuteUnloadPhase();
                e.ReceiveFeedack(NodeTrigger.UnloadDone);
                _lifetime.Dispose();
                _lifetime = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class NodeLifetime : IDisposable
        {
            private readonly NodeHostConfig _nodeHostConfig;
            private readonly INodeHostLogger _logger;
            private readonly ILifetimeScope _scopeContainer;
            private readonly List<ILifecycleEventListener> _lifecycleComponents;
            private readonly RevertableSequence _loadSequence;
            private readonly RevertableSequence _activateSequence;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeLifetime(NodeHostConfig nodeHostConfig, IContainer baseContainer, INodeHostLogger logger)
            {
                _nodeHostConfig = nodeHostConfig;
                _logger = logger;
                _scopeContainer = baseContainer.BeginLifetimeScope();
                _loadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
                _activateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
                _lifecycleComponents = new List<ILifecycleEventListener>();
                
                LoadModules();
                FindLifecycleComponents();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _scopeContainer.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExecuteLoadPhase()
            {
                try
                {
                    _loadSequence.Perform();
                    return true;
                }
                catch ( Exception e )
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
                catch ( Exception e )
                {
                    _logger.NodeActivationError(e);
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExecuteDeactivatePhase()
            {
                try
                {
                    _activateSequence.Revert();
                }
                catch ( Exception e )
                {
                    _logger.NodeDeactivationError(e);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExecuteUnloadPhase()
            {
                try
                {
                    _loadSequence.Revert();
                }
                catch ( Exception e )
                {
                    _logger.NodeUnloadError(e);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<ILifecycleEventListener> LifecycleComponents
            {
                get
                {
                    return _lifecycleComponents;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public INodeHostLogger Logger
            {
                get
                {
                    return _logger;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadModules()
            {
                using ( _logger.LoadingModules() )
                {
                    foreach ( var module in _nodeHostConfig.FrameworkModules.Concat(_nodeHostConfig.ApplicationModules) )
                    {
                        _logger.RegisteringModule(module.Name);

                        try
                        {
                            var assembly = Assembly.LoadFrom(PathUtility.LocalBinPath(module.Assembly));

                            var containerUpdater = new ContainerBuilder();
                            containerUpdater.RegisterAssemblyModules(assembly);
                            containerUpdater.Update(_scopeContainer.ComponentRegistry);
                        }
                        catch ( Exception e )
                        {
                            _logger.FailedToLoadModule(module.Name, e);
                            throw;
                        }
                    }

                    if ( !_nodeHostConfig.ApplicationModules.Any() )
                    {
                        _logger.NoApplicationModulesRegistered();
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FindLifecycleComponents()
            {
                using ( _logger.LookingForLifecycleComponents() )
                {
                    try
                    {
                        IEnumerable<ILifecycleEventListener> foundComponents;

                        if ( _scopeContainer.TryResolve<IEnumerable<ILifecycleEventListener>>(out foundComponents) )
                        {
                            _lifecycleComponents.AddRange(foundComponents);

                            foreach ( var component in _lifecycleComponents )
                            {
                                _logger.FoundLifecycleComponent(component.GetType().ToString());
                            }
                        }
                        else
                        {
                            _logger.NoLifecycleComponentsFound();
                        }

                        if ( _lifecycleComponents.Count == 0 )
                        {
                            _logger.NoLifecycleComponentsFound();
                        }
                    }
                    catch ( Exception e )
                    {
                        _logger.FailedToLoadLifecycleComponents(e);
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : IRevertableSequenceCodeBehind
        {
            private readonly NodeLifetime _ownerLifetime;
            private readonly INodeHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoadSequenceCodeBehind(NodeLifetime ownerLifetime)
            {
                _ownerLifetime = ownerLifetime;
                _logger = _ownerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoading).OnRevert(CallComponentNodeUnloaded);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentLoad).OnRevert(CallComponentUnload);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeLoaded).OnRevert(CallComponentNodeUnloading);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IList<ILifecycleEventListener> GetLifecycleComponents()
            {
                return _ownerLifetime.LifecycleComponents;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeLoading(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeLoading(component.GetType().FullName) )
                {
                    component.NodeLoading();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentLoad(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentLoading(component.GetType().FullName) )
                {
                    component.Load();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeLoaded(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeLoaded(component.GetType().FullName) )
                {
                    component.NodeLoaded();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeUnloading(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeUnloading(component.GetType().FullName) )
                {
                    component.NodeUnloading();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentUnload(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentUnloading(component.GetType().FullName) )
                {
                    component.Unload();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeUnloaded(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeUnloaded(component.GetType().FullName) )
                {
                    component.NodeUnloaded();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActivateSequenceCodeBehind : IRevertableSequenceCodeBehind
        {
            private readonly NodeLifetime _ownerLifetime;
            private readonly INodeHostLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActivateSequenceCodeBehind(NodeLifetime ownerLifetime)
            {
                _ownerLifetime = ownerLifetime;
                _logger = _ownerLifetime.Logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivating).OnRevert(CallComponentNodeDeactivated);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentActivate).OnRevert(CallComponentDeactivate);
                sequence.ForEach(GetLifecycleComponents).OnPerform(CallComponentNodeActivated).OnRevert(CallComponentNodeDeactivating);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IList<ILifecycleEventListener> GetLifecycleComponents()
            {
                return _ownerLifetime.LifecycleComponents;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeActivating(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeActivating(component.GetType().FullName) )
                {
                    component.NodeActivating();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentActivate(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentActivating(component.GetType().FullName) )
                {
                    component.Activate();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeActivated(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeActivated(component.GetType().FullName) )
                {
                    component.NodeActivated();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeDeactivating(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeDeactivating(component.GetType().FullName) )
                {
                    component.NodeDeactivating();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentDeactivate(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentDeactivating(component.GetType().FullName) )
                {
                    component.Deactivate();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallComponentNodeDeactivated(ILifecycleEventListener component, int index, bool isLast)
            {
                using ( _logger.ComponentNodeDeactivated(component.GetType().FullName) )
                {
                    component.NodeDeactivated();
                }
            }
        }
    }
}
