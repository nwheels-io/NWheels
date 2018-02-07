using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NWheels.Kernel.Api.Extensions;

namespace NWheels.Kernel.Api.Exceptions
{
    [Serializable]
    public class StateMachineException : ExplainableExceptionBase
    {
        public Type CodeBehind { get; }
        public string State { get; }
        public string InitialState { get; }
        public string AttemptedState { get; }
        public string Trigger { get; }

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
            this.CodeBehind = codeBehind;
            this.State = state;
            this.InitialState = initialState;
            this.AttemptedState = attemptedState;
            this.Trigger = trigger;
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
                new KeyValuePair<string, string>(_s_keyCodeBehind, this.CodeBehind.FriendlyFullName()),
                new KeyValuePair<string, string>(_s_keyState, this.State),
                new KeyValuePair<string, string>(_s_keyInitialState, this.InitialState),
                new KeyValuePair<string, string>(_s_keyAttemptedState, this.AttemptedState),
                new KeyValuePair<string, string>(_s_keyTrigger, this.Trigger)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_reasonInitialStateNotSet = nameof(InitialStateNotSet);
        private static readonly string _s_reasonInitialStateAlreadyDefined = nameof(InitialStateAlreadyDefined);
        private static readonly string _s_reasonTransitionAlreadyDefined = nameof(TransitionAlreadyDefined);
        private static readonly string _s_reasonTriggerNotValidInCurrentState = nameof(TriggerNotValidInCurrentState);
        private static readonly string _s_reasonDestinationStateNotDefined = nameof(DestinationStateNotDefined);
        private static readonly string _s_keyCodeBehind = nameof(CodeBehind);
        private static readonly string _s_keyState = nameof(State);
        private static readonly string _s_keyInitialState = nameof(InitialState);
        private static readonly string _s_keyAttemptedState = nameof(AttemptedState);
        private static readonly string _s_keyTrigger = nameof(Trigger);

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

        public static StateMachineException TriggerNotValidInCurrentState(Type codeBehind, string state, string trigger)
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
