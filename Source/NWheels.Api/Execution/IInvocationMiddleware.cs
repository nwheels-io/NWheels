using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Execution
{
    public interface IInvocationMiddleware
    {
        Task HandleInvocation(IInvocationMessage invocation);
    }
}
