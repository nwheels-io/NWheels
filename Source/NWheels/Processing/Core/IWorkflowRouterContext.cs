using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
{
    public interface IWorkflowRouterContext
    {
        TWorkItem GetActorWorkItem<TWorkItem>();
        TResult GetActorResult<TResult>();
        TEvent GetReceivedEvent<TEvent>() where TEvent : IWorkflowEvent;
        void EnqueueWorkItem<TWorkItem>(string actorName, TWorkItem workItem);
        bool HasActorResult { get; }
        bool HasReceivedEvent { get; }
    }
}
