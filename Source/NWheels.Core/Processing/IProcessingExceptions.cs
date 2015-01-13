using System;
using NWheels.Exceptions;

namespace NWheels.Core.Processing
{
    public interface IProcessingExceptions : ILocalizableExceptions
    {
        CodeBehindErrorException StateMachineInitialStateNotSet(Type codeBehindType);
        CodeBehindErrorException StateMachineStateAlreadyDefined(Type codeBehindType, object stateValue);
        CodeBehindErrorException StateMachineInitialStateAlreadyDefined(
            Type codeBehindType, 
            object initialStateValue, 
            object attemptedStateValue);
        CodeBehindErrorException StateMachineTransitionAlreadyDefined(Type codeBehindType, object stateValue, object triggerValue);
        CodeBehindErrorException StateMachineTransitionNotDefined(Type codeBehindType, object stateValue, object triggerValue);
    }
}
