using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IStateMachineBuilder<TState, TTrigger>
    {
        IStateMachineStateBuilder<TState, TTrigger> State(TState value);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStateMachineStateBuilder<TState, TTrigger>
    {
        IStateMachineStateBuilder<TState, TTrigger> SetAsInitial();
        IStateMachineTransitionBuilder<TState, TTrigger> OnTrigger(TTrigger trigger);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStateMachineTransitionBuilder<TState, TTrigger>
    {
        IStateMachineStateBuilder<TState, TTrigger> TransitionTo(TState destination);
    }
}
