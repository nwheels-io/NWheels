using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IWorkflowActor
    {
        string Name { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWorkflowActor<TWorkItem> : IWorkflowActor
    {
        void Execute(IWorkflowActorContext context, TWorkItem workItem);
    }
}
