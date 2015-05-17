using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
{
    public abstract class WorkflowEventBase<TKey, TPayload> : IWorkflowEvent<TKey, TPayload>
    {
        protected WorkflowEventBase(TKey key, TPayload payload)
        {
            this.Key = key;
            this.Payload = payload;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IWorkflowEvent.KeyObject
        {
            get { return this.Key; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TKey Key { get; private set; }
        public TPayload Payload { get; private set; }
    }
}
