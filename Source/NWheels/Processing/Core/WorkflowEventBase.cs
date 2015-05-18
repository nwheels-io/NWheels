using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
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
