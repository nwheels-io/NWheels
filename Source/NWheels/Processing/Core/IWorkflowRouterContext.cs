using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
{
    public interface IWorkflowRouterContext : IWorkflowActorSiteContext
    {
        TWorkItem GetActorWorkItem<TWorkItem>();
        TResult GetActorResult<TResult>();
        TEvent GetReceivedEvent<TEvent>() where TEvent : IWorkflowEvent;
        bool HasActorResult { get; }
        bool HasReceivedEvent { get; }
    }
}
