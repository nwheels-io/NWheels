using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing
{
    public class StateMachineTriggerEvent<TTrigger> : WorkflowEventBase<Guid>
    {
        private readonly TTrigger _trigger;
        private readonly object _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachineTriggerEvent(Guid workflowInstanceId, TTrigger trigger, object context)
            : base(workflowInstanceId)
        {
            _trigger = trigger;
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TTrigger Trigger
        {
            get { return _trigger; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object Context
        {
            get { return _context; }
        }
    }
}
