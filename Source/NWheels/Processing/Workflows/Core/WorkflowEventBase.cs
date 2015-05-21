using System;

namespace NWheels.Processing.Workflows.Core
{
    public abstract class WorkflowEventBase<TKey> : IWorkflowEvent<TKey>
    {
        protected WorkflowEventBase(TKey key)
        {
            this.Key = key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public virtual Type GetEventType()
        {
            return this.GetType();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual object GetEventKey()
        {
            return this.Key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual WorkflowEventStatus GetEventStatus()
        {
            return WorkflowEventStatus.Received;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TKey Key { get; private set; }
    }
}
