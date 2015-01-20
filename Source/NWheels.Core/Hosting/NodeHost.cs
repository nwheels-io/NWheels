using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Hapil;
using NWheels.Core.Logging;
using NWheels.Core.Processing;
using NWheels.Hosting;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Core.Hosting
{
    public class NodeHost : INodeHost
    {
        private readonly NodeHostConfig _nodeHostConfig;
        private readonly DynamicModule _dynamicModule;
        private readonly ConventionObjectFactory _loggerFactory;
        private readonly IContainer _container;
        private readonly StateMachine<NodeState, NodeTrigger> _stateMachine;
        //private readonly List<Exception> _nodeHostErrors;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHost(NodeHostConfig config)
        {
            _nodeHostConfig = config;
            _dynamicModule = new DynamicModule(simpleName: "NWheels.RunTimeTypes", allowSave: true, saveDirectory: PathUtility.LocalBinPath());
            _loggerFactory = new ConventionObjectFactory(_dynamicModule, new ApplicationEventLoggerConvention());

            _container = BuildInitialContainer();
            
            _stateMachine = new StateMachine<NodeState, NodeTrigger>(
                new StateMachineCodeBehind(this), 
                logger: _container.Resolve<StateMachine<NodeState, NodeTrigger>.ILogger>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
            _stateMachine.ReceiveTrigger(NodeTrigger.Load);

            if ( _stateMachine.CurrentState != NodeState.Standby )
            {
                throw new Exception("Node load failed");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activate()
        {
            _stateMachine.ReceiveTrigger(NodeTrigger.Activate);

            if ( _stateMachine.CurrentState != NodeState.Active )
            {
                throw new Exception("Node activate failed");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadAndActivate()
        {
            Load();
            Activate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivate()
        {
            _stateMachine.ReceiveTrigger(NodeTrigger.Deactivate);

            if ( _stateMachine.CurrentState != NodeState.Standby )
            {
                throw new Exception("Node deactivate failed");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unload()
        {
            _stateMachine.ReceiveTrigger(NodeTrigger.Unload);

            if ( _stateMachine.CurrentState != NodeState.Down )
            {
                throw new Exception("Node unload failed");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeactivateAndUnload()
        {
            Deactivate();
            Unload();
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

        //public event EventHandler<NodeHostContainerEventArgs> RegisteringHostComponents;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteLoadPhase()
        {
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteActivatePhase()
        {
            return false;
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

        private static IContainer BuildInitialContainer()
        {
            var builder = new ContainerBuilder();




            return builder.Build(ContainerBuildOptions.IgnoreStartableComponents);
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
    }
}
