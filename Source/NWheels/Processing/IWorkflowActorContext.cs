using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NWheels.Processing
{
    public interface IWorkflowActorContext
    {
        void EnqueueWorkItem<TWorkItem>(string actorName, TWorkItem workItem);
    }
}
