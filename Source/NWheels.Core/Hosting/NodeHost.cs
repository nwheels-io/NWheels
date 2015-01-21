using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Hapil;
using NWheels.Conventions;
using NWheels.Core.Conventions;
using NWheels.Core.Logging;
using NWheels.Core.Processing;
using NWheels.Exceptions;
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
        private readonly ILogger _logger;
        private readonly StateMachine<NodeState, NodeTrigger> _stateMachine;
        //private readonly List<Exception> _nodeHostErrors;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHost(NodeHostConfig config)
        {
            _nodeHostConfig = config;
            
            _dynamicModule = new DynamicModule(
                simpleName: "NWheels.RunTimeTypes", 
                allowSave: true, 
                saveDirectory: PathUtility.LocalBinPath());
            
            _baseContainer = BuildBaseContainer();
            
            _stateMachine = new StateMachine<NodeState, NodeTrigger>(
                new StateMachineCodeBehind(this), 
                _baseContainer.Resolve<Auto<StateMachine<NodeState, NodeTrigger>.ILogger>>());

            _logger = _baseContainer.ResolveAuto<ILogger>();
            _logger.NodeHostInitializing(config.ApplicationName, config.NodeName, this.GetType().Assembly.GetName().Version).Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterHostSpecificComponents(Action<ContainerBuilder> registrar)
        {
            var builder = new ContainerBuilder();
            registrar(builder);

            builder.Update(_baseContainer);
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

        private bool ExecuteLoadPhase()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteActivatePhase()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteDeactivatePhase()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteUnloadPhase()
        {
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IContainer BuildBaseContainer()
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

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineCodeBehind : IStateMachineCodeBehind<NodeState, NodeTrigger>
        {
            private readonly NodeHost _owner;

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
                var success = _owner.ExecuteLoadPhase();
                e.ReceiveFeedack(success ? NodeTrigger.LoadSuccess : NodeTrigger.LoadFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                var success = _owner.ExecuteActivatePhase();
                e.ReceiveFeedack(success ? NodeTrigger.ActivateSuccess : NodeTrigger.ActivateFailure);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _owner.ExecuteDeactivatePhase();
                e.ReceiveFeedack(NodeTrigger.DeactivateDone);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<NodeState, NodeTrigger> e)
            {
                _owner.ExecuteUnloadPhase();
                e.ReceiveFeedack(NodeTrigger.UnloadDone);
            }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogThread(ThreadTaskType.StartUp)]
            ILogActivity NodeHostInitializing(string application, string node, Version hostVersion);

            [LogThread(ThreadTaskType.StartUp)]
            ILogActivity NodeStartingUp();

            [LogThread(ThreadTaskType.StartUp)]
            ILogActivity NodeLoading();
            
            [LogInfo]
            void NodeSuccessfullyLoaded();
            
            [LogError]
            NodeHostException NodeHasFailedToLoad();

            [LogThread(ThreadTaskType.StartUp)]
            ILogActivity NodeActivating();
            
            [LogInfo]
            void NodeSuccessfullyActivated();
            
            [LogError]
            NodeHostException NodeHasFailedToActivate();

            [LogThread(ThreadTaskType.ShutDown)]
            ILogActivity NodeShuttingDown();

            [LogThread(ThreadTaskType.ShutDown)]
            ILogActivity NodeDeactivating();

            [LogError]
            NodeHostException NodeHasFailedToDeactivate();
            
            [LogInfo]
            void NodeDeactivated();

            [LogThread(ThreadTaskType.ShutDown)]
            ILogActivity NodeUnloading();
            
            [LogInfo]
            void NodeUnloaded();

            [LogError]
            NodeHostException NodeHasFailedToUnload();
        }
    }
}
