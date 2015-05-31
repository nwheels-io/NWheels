using System;
using System.Collections.Generic;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows.Impl
{
    internal class StateActor<TState, TTrigger> : 
        IWorkflowActor<StateTriggerWorkItem<TState, TTrigger>>, 
        IWorkflowRouter,
        IStateMachineStateBuilder<TState, TTrigger>
    {
        private readonly TState _value;
        private readonly IStateMachineCodeBehind<TState, TTrigger> _codeBehind;
        private readonly TransientStateMachine<TState, TTrigger>.ILogger _logger;
        private readonly Dictionary<TTrigger, StateTransition> _transitionByTrigger = new Dictionary<TTrigger, StateTransition>();
        private TimeSpan? _timeout;
        private EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> _onEntered;
        private EventHandler<StateMachineEventArgs<TState, TTrigger>> _onLeaving;
        private EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> _onTimeout;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateActor(TState value, IStateMachineCodeBehind<TState, TTrigger> codeBehind, TransientStateMachine<TState, TTrigger>.ILogger logger)
        {
            _codeBehind = codeBehind;
            _value = value;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute(IWorkflowActorContext context, StateTriggerWorkItem<TState, TTrigger> workItem)
        {
            var completed = false;

            if ( _onEntered != null )
            {
                _onEntered(this, workItem.EventArgs);

                if ( workItem.EventArgs.HasFeedback )
                {
                    HandleTrigger(context, workItem.EventArgs.Feedback, workItem.EventArgs.Context);
                    completed = true;
                }
            }

            if ( !completed )
            {
                context.AwaitEvent<StateMachineTriggerEvent<TTrigger>, Guid>(
                    key: context.WorkflowInstance.InstanceId, 
                    timeout: _timeout.GetValueOrDefault(TimeSpan.FromDays(1)));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Route(IWorkflowRouterContext context)
        {
            if ( context.HasReceivedEvent<StateMachineTriggerEvent<TTrigger>>() )
            {
                var receivedEvent = context.GetReceivedEvent<StateMachineTriggerEvent<TTrigger>>();
                HandleTrigger(context, receivedEvent.Trigger, eventArgsContext: receivedEvent.Context);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetCookieType()
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IStateMachineStateBuilder<TState, TTrigger> SetAsInitial()
        {
            this.IsInitialState = true;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IStateMachineStateBuilder<TState, TTrigger> OnEntered(EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> handler)
        {
            if ( _onEntered != null )
            {
                _onEntered = handler;
            }
            else
            {
                _onEntered += handler;
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IStateMachineStateBuilder<TState, TTrigger> OnTimeout(
            TimeSpan timeout,
            EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> handler,
            bool recurring = false)
        {
            _timeout = timeout;

            if ( _onTimeout != null )
            {
                _onTimeout = handler;
            }
            else
            {
                _onTimeout += handler;
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IStateMachineStateBuilder<TState, TTrigger> OnLeaving(EventHandler<StateMachineEventArgs<TState, TTrigger>> handler)
        {
            if ( _onLeaving != null )
            {
                _onLeaving = handler;
            }
            else
            {
                _onLeaving += handler;
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IStateMachineTransitionBuilder<TState, TTrigger> OnTrigger(TTrigger trigger)
        {
            if ( _transitionByTrigger.ContainsKey(trigger) )
            {
                throw _logger.TransitionAlreadyDefined(_codeBehind.GetType(), _value, trigger);
            }

            return new StateTransition(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsInitialState { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TState Value
        {
            get { return _value; }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void HandleTrigger(IWorkflowActorSiteContext context, TTrigger trigger, object eventArgsContext)
        {
            var transition = ValidateTransition(trigger);
            
            var args = new StateMachineFeedbackEventArgs<TState, TTrigger>(
                @from: _value,
                to: transition.Destination,
                trigger: trigger,
                context: eventArgsContext);

            if ( _onLeaving != null )
            {
                _onLeaving(this, args);
            }

            transition.PerformTransitioning(args);

            context.EnqueueWorkItem(
                actorName: transition.Destination.ToString(), 
                workItem: new StateTriggerWorkItem<TState, TTrigger>(args));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private StateTransition ValidateTransition(TTrigger trigger)
        {
            StateTransition transition;

            if ( _transitionByTrigger.TryGetValue(trigger, out transition) )
            {
                return transition;
            }
            else
            {
                throw _logger.TransitionNotDefined(_codeBehind.GetType(), _value, trigger);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateTransition : IStateMachineTransitionBuilder<TState, TTrigger>
        {
            private readonly StateActor<TState, TTrigger> _origin;
            private TState _destination;
            private EventHandler<StateMachineEventArgs<TState, TTrigger>> _onTransitioning;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateTransition(StateActor<TState, TTrigger> origin)
            {
                _origin = origin;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void PerformTransitioning(StateMachineEventArgs<TState, TTrigger> args)
            {
                if ( _onTransitioning != null )
                {
                    _onTransitioning(this, args);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IStateMachineStateBuilder<TState, TTrigger> TransitionTo(
                TState destination, 
                EventHandler<StateMachineEventArgs<TState, TTrigger>> onTransitioning = null)
            {
                _destination = destination;
                _onTransitioning = onTransitioning;

                return _origin;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public TState Destination
            {
                get { return _destination; }
            }
        }
    }
}
