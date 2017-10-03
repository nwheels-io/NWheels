using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NWheels.Kernel.Api.Exceptions
{
    [Serializable]
    public class StateMachineException : ExplainableExceptionBase
    {
        private readonly Type _codeBehind;
        private readonly string _state;
        private readonly string _initialState;
        private readonly string _attemptedState;
        private readonly string _trigger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private StateMachineException(
            string reason,
            Type codeBehind, 
            string state = null, 
            string initialState = null, 
            string attemptedState = null, 
            string trigger = null)
            : base(reason)
        {
            _codeBehind = codeBehind;
            _state = state;
            _initialState = initialState;
            _attemptedState = attemptedState;
            _trigger = trigger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected StateMachineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
        {
            return new[] {
                new KeyValuePair<string, string>(_s_keyCodeBehind, _codeBehind.FullName),
                new KeyValuePair<string, string>(_s_keyState, _state),
                new KeyValuePair<string, string>(_s_keyInitialState, _initialState),
                new KeyValuePair<string, string>(_s_keyAttemptedState, _attemptedState),
                new KeyValuePair<string, string>(_s_keyTrigger, _trigger)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_reasonInitialStateNotSet = nameof(InitialStateNotSet);
        private static readonly string _s_reasonInitialStateAlreadyDefined = nameof(InitialStateAlreadyDefined);
        private static readonly string _s_reasonTransitionAlreadyDefined = nameof(TransitionAlreadyDefined);
        private static readonly string _s_reasonTriggerNotValidInCurrentState = nameof(TriggetNotValidInCurrentState);
        private static readonly string _s_reasonDestinationStateNotDefined = nameof(DestinationStateNotDefined);
        private static readonly string _s_keyCodeBehind = "codeBehind";
        private static readonly string _s_keyState = "state";
        private static readonly string _s_keyInitialState = "initialState";
        private static readonly string _s_keyAttemptedState = "attemptedState";
        private static readonly string _s_keyTrigger = "trigger";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static StateMachineException InitialStateNotSet(Type codeBehind)
        {
            return new StateMachineException(_s_reasonInitialStateNotSet, codeBehind);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static StateMachineException InitialStateAlreadyDefined(Type codeBehind, string initialState, string attemptedState)
        {
            return new StateMachineException(_s_reasonInitialStateAlreadyDefined, codeBehind, initialState: initialState, attemptedState: attemptedState);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static StateMachineException TransitionAlreadyDefined(Type codeBehind, string state, string trigger)
        {
            return new StateMachineException(_s_reasonTransitionAlreadyDefined, codeBehind, state: state, trigger: trigger);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static StateMachineException TriggetNotValidInCurrentState(Type codeBehind, string state, string trigger)
        {
            return new StateMachineException(_s_reasonTriggerNotValidInCurrentState, codeBehind, state: state, trigger: trigger);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static StateMachineException DestinationStateNotDefined(Type codeBehind, string state)
        {
            return new StateMachineException(_s_reasonDestinationStateNotDefined, codeBehind, state: state);
        }
    }
}
