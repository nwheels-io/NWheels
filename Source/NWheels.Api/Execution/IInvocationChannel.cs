using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Execution
{
    public interface IInvocationChannel
    {
        Task ScheduledInvoke(IInvocationMessage invocation);
    }
}
