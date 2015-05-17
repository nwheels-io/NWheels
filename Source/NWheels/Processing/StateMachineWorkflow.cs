using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;
using NWheels.Processing.Impl;

namespace NWheels.Processing
{
    public abstract class StateMachineWorkflow<TState, TTrigger, TDataEntity> : 
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected StateMachineWorkflow(IStateMachineCodeBehind<TState, TTrigger> codeBehind, TransientStateMachine<TState, TTrigger>.ILogger logger)
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
                builder.AddActor(actorAndRouter.Value.ToString(), priority, actor: actorAndRouter, router: actorAndRouter);
            }
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

        public virtual void OnInitialize(TDataEntity initialData)
        {
            var initializable = (_codeBehind as IInitializableWorkflowCodeBehind<TDataEntity>);

            if ( initializable != null )
            {
                initializable.OnInitialize(initialData);
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

        public virtual void OnTimeout(TDataEntity savedData, ref bool isFailure)
        {
            var suspendable = (_codeBehind as ISuspendableWorkflowCodeBehind<TDataEntity>);

            

            if ( suspendable != null )
            {
                suspendable.OnTimeout(savedData, ref isFailure);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnBuildStateMachine(IStateMachineBuilder<TState, TTrigger> builder);
    }
}
