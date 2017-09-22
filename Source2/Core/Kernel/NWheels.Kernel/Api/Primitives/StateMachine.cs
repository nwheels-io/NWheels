using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Kernel.Api.Primitives
{
    public static class StateMachine
    {
        public static StateMachine<TState, TTrigger> CreateFrom<TState, TTrigger>(IStateMachineCodeBehind<TState, TTrigger> codeBehind)
        {
            return new StateMachine<TState, TTrigger>(codeBehind);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class StateMachine<TState, TTrigger> : IStateMachineBuilder<TState, TTrigger>
    {
        private readonly IStateMachineCodeBehind<TState, TTrigger> _codeBehind;
        private readonly Dictionary<TState, MachineState> _states;
        private MachineState _currentState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachine(IStateMachineCodeBehind<TState, TTrigger> codeBehind)
        {
            if (codeBehind == null)
            {
                throw new ArgumentNullException(nameof(codeBehind));
            }

            _codeBehind = codeBehind;
            _states = new Dictionary<TState, MachineState>();

            _codeBehind.BuildStateMachine(this);

            if (_currentState == null)
            {
                throw StateMachineException.InitialStateNotSet(_codeBehind.GetType());
            }

            var eventArgs = new StateMachineFeedbackEventArgs<TState, TTrigger>(_currentState.Value);
            _currentState.Enter(eventArgs);

            if (eventArgs.HasFeedback)
            {
                ReceiveTrigger(eventArgs.Feedback);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IStateMachineStateBuilder<TState, TTrigger> IStateMachineBuilder<TState, TTrigger>.State(TState value)
        {
            if (_states.TryGetValue(value, out MachineState existingState))
            {
                return existingState;
            }

            var newState = new MachineState(this, value);
            _states.Add(value, newState);

            return newState;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStateMachineBuilder<TState, TTrigger>.RestoreState(TState value)
        {
            _currentState = _states[value];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReceiveTrigger(TTrigger trigger)
        {
            ReceiveTrigger(trigger, context: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReceiveTrigger(TTrigger trigger, object context)
        {
            var stateChanged = false;

            try
            {
                StateMachineFeedbackEventArgs<TState, TTrigger> eventArgs;

                do
                {
                    eventArgs = PerformTrigger(trigger, context);
                    trigger = eventArgs.Feedback;
                    stateChanged = true;
                } while (eventArgs.HasFeedback);
            }
            finally
            {
                if (stateChanged)
                {
                    RaiseCurrentStateChanged();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TState CurrentState
        {
            get
            {
                return _currentState.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler CurrentStateChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TrySetInitialState(MachineState machineState)
        {
            if (_currentState != null)
            {
                throw StateMachineException.InitialStateAlreadyDefined(_codeBehind.GetType(), _currentState.Value.ToString(), machineState.Value.ToString());
            }

            _currentState = machineState;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private StateMachineFeedbackEventArgs<TState, TTrigger> PerformTrigger(TTrigger trigger, object context)
        {
            var currentState = _currentState;
            var transition = currentState.TryGetTransition(trigger);

            if (transition == null)
            {
                throw StateMachineException.TriggetNotValidInCurrentState(_codeBehind.GetType(), currentState.Value.ToString(), trigger.ToString());
            }

            var eventArgs = new StateMachineFeedbackEventArgs<TState, TTrigger>(_currentState.Value, transition.DestinationStateValue, trigger, context);

            _currentState.Leave(eventArgs);

            try
            {
                transition.RaiseTransitioning(eventArgs);
            }
            catch
            {
                _currentState.Enter(eventArgs);
                throw;
            }

            _currentState = _states[transition.DestinationStateValue];
            _currentState.Enter(eventArgs);

            return eventArgs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RaiseCurrentStateChanged()
        {
            if (CurrentStateChanged != null)
            {
                CurrentStateChanged(this, EventArgs.Empty);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class MachineState : IStateMachineStateBuilder<TState, TTrigger>
        {
            private readonly StateMachine<TState, TTrigger> _ownerMachine;
            private readonly TState _value;
            private readonly Dictionary<TTrigger, StateTransition> _transitions;
            private EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> _onEntered = null;
            private EventHandler<StateMachineEventArgs<TState, TTrigger>> _onLeaving = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MachineState(StateMachine<TState, TTrigger> ownerMachine, TState value)
            {
                _value = value;
                _ownerMachine = ownerMachine;
                _transitions = new Dictionary<TTrigger, StateTransition>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineStateBuilder<TState, TTrigger> IStateMachineStateBuilder<TState, TTrigger>.SetAsInitial()
            {
                _ownerMachine.TrySetInitialState(this);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineStateBuilder<TState, TTrigger> IStateMachineStateBuilder<TState, TTrigger>.OnEntered(
                EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> handler)
            {
                _onEntered += handler;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //TODO: implement timeout
            //IStateMachineStateBuilder<TState, TTrigger> IStateMachineStateBuilder<TState, TTrigger>.OnTimeout(
            //    TimeSpan timeout,
            //    EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> handler,
            //    bool recurring)
            //{
            //    //TODO: implement timeout
            //    return this;
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineStateBuilder<TState, TTrigger> IStateMachineStateBuilder<TState, TTrigger>.OnLeaving(
                EventHandler<StateMachineEventArgs<TState, TTrigger>> handler)
            {
                _onLeaving += handler;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineTransitionBuilder<TState, TTrigger> IStateMachineStateBuilder<TState, TTrigger>.OnTrigger(TTrigger trigger)
            {
                if (_transitions.ContainsKey(trigger))
                {
                    throw StateMachineException.TransitionAlreadyDefined(_ownerMachine._codeBehind.GetType(), _value.ToString(), trigger.ToString());
                }

                return new StateTransition(_ownerMachine, this, trigger);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateTransition TryGetTransition(TTrigger trigger)
            {
                StateTransition transition;

                if (_transitions.TryGetValue(trigger, out transition))
                {
                    return transition;
                }
                else
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddTransition(StateTransition transition)
            {
                _transitions.Add(transition.Trigger, transition);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Enter(StateMachineFeedbackEventArgs<TState, TTrigger> args)
            {
                if (_onEntered != null)
                {
                    _onEntered(_ownerMachine, args);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Leave(StateMachineEventArgs<TState, TTrigger> args)
            {
                if (_onLeaving != null)
                {
                    _onLeaving(_ownerMachine, args);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TState Value
            {
                get { return _value; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateTransition : IStateMachineTransitionBuilder<TState, TTrigger>
        {
            private readonly StateMachine<TState, TTrigger> _ownerMachine;
            private readonly MachineState _ownerState;
            private readonly TTrigger _trigger;
            private TState _destinationStateValue;
            private EventHandler<StateMachineEventArgs<TState, TTrigger>> _onTransitioning = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateTransition(
                StateMachine<TState, TTrigger> ownerMachine,
                MachineState ownerState,
                TTrigger trigger)
            {
                _ownerMachine = ownerMachine;
                _ownerState = ownerState;
                _trigger = trigger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineStateBuilder<TState, TTrigger> IStateMachineTransitionBuilder<TState, TTrigger>.TransitionTo(
                TState destination,
                EventHandler<StateMachineEventArgs<TState, TTrigger>> onTransitioning)
            {
                _destinationStateValue = destination;
                _ownerState.AddTransition(this);
                _onTransitioning = onTransitioning;

                return _ownerState;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RaiseTransitioning(StateMachineEventArgs<TState, TTrigger> args)
            {
                if (_onTransitioning != null)
                {
                    _onTransitioning(_ownerMachine, args);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TTrigger Trigger
            {
                get { return _trigger; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TState DestinationStateValue
            {
                get { return _destinationStateValue; }
            }
        }
    }
}
