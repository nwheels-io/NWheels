using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
{
    public interface IWorkflowRouter
    {
        void Route(IWorkflowRouterContext context);
    }
}
