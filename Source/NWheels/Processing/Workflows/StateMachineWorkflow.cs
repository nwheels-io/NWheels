using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Processing.Workflows.Core;
using NWheels.Processing.Workflows.Impl;

namespace NWheels.Processing.Workflows
{
    public class StateMachineWorkflow<TState, TTrigger, TDataEntity> : 
        IStateMachineBuilder<TState, TTrigger>,
        IWorkflowCodeBehind,
        IWorkflowCodeBehindLifecycle,
        IInitializableWorkflowCodeBehind<TDataEntity>,
        ISuspendableWorkflowCodeBehind<TDataEntity>
        where TDataEntity : class, IStateMachineInstanceEntity<TState>
    {
        private readonly TransientStateMachine<TState, TTrigger>.ILogger _logger;
        private readonly IStateMachineCodeBehind<TState, TTrigger> _codeBehind;
        private readonly Dictionary<TState, StateActor<TState, TTrigger>> _actorsByState;
        private StateActor<TState, TTrigger> _initialState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachineWorkflow(IStateMachineCodeBehind<TState, TTrigger> codeBehind, TransientStateMachine<TState, TTrigger>.ILogger logger)
        {
            _codeBehind = codeBehind;
            _logger = logger;
            _actorsByState = new Dictionary<TState, StateActor<TState, TTrigger>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IWorkflowCodeBehind.OnBuildWorkflow(IWorkflowBuilder builder)
        {
            _codeBehind.BuildStateMachine(this);

            var allActors = _actorsByState.Values.ToList();
            allActors.Sort(comparison: (x, y) => x.IsInitialState.CompareTo(y.IsInitialState));

            var priority = 0;

            foreach ( var actorAndRouter in allActors )
            {
                priority++;
                builder.AddActor(actorAndRouter.Value.ToString(), priority, actor: actorAndRouter, router: actorAndRouter, isInitial: priority == 0);
            }

            _initialState = allActors.FirstOrDefault();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        IStateMachineStateBuilder<TState, TTrigger> IStateMachineBuilder<TState, TTrigger>.State(TState value)
        {
            if ( object.ReferenceEquals(null, value) )
            {
                throw new ArgumentNullException("value");
            }

            if ( _actorsByState.ContainsKey(value) )
            {
                throw _logger.StateAlreadyDefined(_codeBehind.GetType(), value);
            }

            var actor = new StateActor<TState, TTrigger>(value, _codeBehind, _logger);
            _actorsByState.Add(value, actor);

            return actor;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void OnInitialize(TDataEntity initialData, IWorkflowInitializer initializer)
        {
            var initializable = (_codeBehind as IInitializableWorkflowCodeBehind<TDataEntity>);

            if ( initializable != null )
            {
                initializable.OnInitialize(initialData, initializer);
            }

            if ( _initialState != null )
            {
                initializer.SetInitialWorkItem(
                    new StateTriggerWorkItem<TState, TTrigger>(new StateMachineFeedbackEventArgs<TState, TTrigger>(_initialState.Value)));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void OnStart()
        {
            var lifecycle = (_codeBehind as IWorkflowCodeBehindLifecycle);

            if ( lifecycle != null )
            {
                lifecycle.OnStart();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void OnSuspend(TDataEntity dataToSave)
        {
            var suspendable = (_codeBehind as ISuspendableWorkflowCodeBehind<TDataEntity>);

            if ( suspendable != null )
            {
                suspendable.OnSuspend(dataToSave);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void OnResume(TDataEntity savedData)
        {
            var suspendable = (_codeBehind as ISuspendableWorkflowCodeBehind<TDataEntity>);

            if ( suspendable != null )
            {
                suspendable.OnResume(savedData);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void OnComplete()
        {
            var lifecycle = (_codeBehind as IWorkflowCodeBehindLifecycle);

            if ( lifecycle != null )
            {
                lifecycle.OnComplete();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void OnFail(Exception error)
        {
            var lifecycle = (_codeBehind as IWorkflowCodeBehindLifecycle);

            if ( lifecycle != null )
            {
                lifecycle.OnFail(error);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void OnFinalize()
        {
            var lifecycle = (_codeBehind as IWorkflowCodeBehindLifecycle);

            if ( lifecycle != null )
            {
                lifecycle.OnFinalize();
            }
        }
    }
}
