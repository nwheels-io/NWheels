using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Impl
{
    public class StateTriggerWorkItem<TState, TTrigger>
    {
        public StateTriggerWorkItem(StateMachineFeedbackEventArgs<TState, TTrigger> eventArgs)
        {
            this.EventArgs = eventArgs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachineFeedbackEventArgs<TState, TTrigger> EventArgs { get; private set; }
    }
}
