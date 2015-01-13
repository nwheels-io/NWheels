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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MachineState(StateMachine<TState, TTrigger> owner, TState value)
            {
                _value = value;
                _owner = owner;
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
                return new TransitionBuilder(this, trigger);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TState Value
            {
                get { return _value; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TransitionBuilder : IStateMachineTransitionBuilder<TState, TTrigger>
        {
            private readonly MachineState _owner;
            private readonly TTrigger _trigger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TransitionBuilder(MachineState owner, TTrigger trigger)
            {
                _trigger = trigger;
                _owner = owner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IStateMachineStateBuilder<TState, TTrigger> IStateMachineTransitionBuilder<TState, TTrigger>.TransitionTo(TState destination)
            {
                return _owner;
            }
        }
    }
}
