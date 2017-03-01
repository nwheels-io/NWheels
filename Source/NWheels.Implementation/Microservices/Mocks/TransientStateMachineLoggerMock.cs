using System;

namespace NWheels.Microservices.Mocks
{
    internal class TransientStateMachineLoggerMock<TState, TTrigger> : NWheels.Orchestration.TransientStateMachine<TState, TTrigger>.ILogger
    {
        public Exception InitialStateAlreadyDefined(Type codeBehind, TState initialState, TState attemptedState)
        {
            throw new NotImplementedException();
        }

        public Exception InitialStateNotSet(Type codeBehind)
        {
            throw new NotImplementedException();
        }

        public Exception StateAlreadyDefined(Type codeBehind, TState state)
        {
            throw new NotImplementedException();
        }

        public Exception TransitionAlreadyDefined(Type codeBehind, TState state, TTrigger trigger)
        {
            throw new NotImplementedException();
        }

        public Exception TransitionNotDefined(Type codeBehind, TState state, TTrigger trigger)
        {
            throw new NotImplementedException();
        }
    }
}
