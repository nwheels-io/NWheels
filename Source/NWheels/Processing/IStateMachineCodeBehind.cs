using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IStateMachineCodeBehind<TState, TTrigger>
    {
        void BuildStateMachine(IStateMachineBuilder<TState, TTrigger> machine);
    }
}
