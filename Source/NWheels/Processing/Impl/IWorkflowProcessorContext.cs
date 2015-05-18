using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing.Impl
{
    internal interface IWorkflowProcessorContext
    {
        void AwaitEvent(Type eventType, object eventKey, TimeSpan timeout);
        IWorkflowInstance WorkflowInstance { get; }
    }
}
