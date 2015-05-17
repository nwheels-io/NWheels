using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing
{
    public class StateMachineTriggerEvent<TTrigger> : WorkflowEventBase<Guid, object>
    {
        private readonly TTrigger _trigger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachineTriggerEvent(Guid workflowInstanceId, TTrigger trigger, object context)
            : base(workflowInstanceId, context)
        {
            _trigger = trigger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TTrigger Trigger
        {
            get { return _trigger; }
        }
    }
}
