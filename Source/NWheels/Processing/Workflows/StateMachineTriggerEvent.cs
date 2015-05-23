using System;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows
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
