using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;
using NWheels.Processing;

namespace NWheels.Core.Processing
{
    public class StateMachine<TState, TTrigger> : IStateMachineBuilder<TState, TTrigger>
    {
        private readonly IStateMachineCodeBehind<TState, TTrigger> _codeBehind;
        private readonly IProcessingExceptions _errors;
        private readonly Dictionary<TState, MachineState> _states;
        private volatile MachineState _currentState;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachine(IStateMachineCodeBehind<TState, TTrigger> codeBehind, IProcessingExceptions errors)
        {
            _errors = errors;
            _codeBehind = codeBehind;
            _states = new Dictionary<TState, MachineState>();

            _codeBehind.BuildStateMachine(this);

            if ( _currentState == null )
            {
                throw _errors.StateMachineInitialStateNotSet(_codeBehind.GetType());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IStateMachineStateBuilder<TState, TTrigger> IStateMachineBuilder<TState, TTrigger>.State(TState value)
        {
            if ( _states.ContainsKey(value) )
            {
                throw _errors.StateMachineStateAlreadyDefined(_codeBehind.GetType(), value);
            }

            var newState = new MachineState(this, value);
            _states.Add(value, newState);

            return newState;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReceiveTrigger(TTrigger trigger)
        {
            var currentState = _currentState;
            var transition = currentState.ValidateTransition(trigger);

            if ( transition == null )
            {
                throw _errors.StateMachineTransitionAlreadyDefined(_codeBehind.GetType(), currentState.Value, trigger);
            }

            _currentState = _states[transition.DestinationStateValue];
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

        private void TrySetInitialState(MachineState machineState)
        {
            if ( _currentState != null )
            {
                throw _errors.StateMachineInitialStateAlreadyDefined(_codeBehind.GetType(), _currentState.Value, machineState.Value);
            }

            _currentState = machineState;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class MachineState : IStateMachineStateBuilder<TState, TTrigger>
        {
            private readonly StateMachine<TState, TTrigger> _owner;
            private readonly TState _value;
            private readonly Dictionary<TTrigger, StateTransition> _transitions;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MachineState(StateMachine<TState, TTrigger> owner, TState value)
            {
                _value = value;
                _owner = owner;
                _transitions = new Dictionary<TTrigger, StateTransition>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineStateBuilder<TState, TTrigger> IStateMachineStateBuilder<TState, TTrigger>.SetAsInitial()
            {
                _owner.TrySetInitialState(this);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineTransitionBuilder<TState, TTrigger> IStateMachineStateBuilder<TState, TTrigger>.OnTrigger(TTrigger trigger)
            {
                if ( _transitions.ContainsKey(trigger) )
                {
                    throw _owner._errors.StateMachineTransitionAlreadyDefined(_owner._codeBehind.GetType(), _value, trigger);
                }


                return new StateTransition(_owner, this, trigger);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateTransition ValidateTransition(TTrigger trigger)
            {
                StateTransition transition;

                if ( _transitions.TryGetValue(trigger, out transition) )
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

            IStateMachineStateBuilder<TState, TTrigger> IStateMachineTransitionBuilder<TState, TTrigger>.TransitionTo(TState destination)
            {
                _destinationStateValue = destination;
                _ownerState.AddTransition(this);
                
                return _ownerState;
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
