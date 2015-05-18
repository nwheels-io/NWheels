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
        bool HasActorResult<TResult>();
        TResult GetActorResult<TResult>();
        bool HasReceivedEvent<TEvent>() where TEvent : IWorkflowEvent;
        TEvent GetReceivedEvent<TEvent>() where TEvent : IWorkflowEvent;
    }
}
