using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Execution
{
    public interface IInvocationMessage
    {
        Task Invoke(object target);
        Task CompletionFuture { get; }
        Type TargetType { get; }
        MethodInfo TargetMethod { get; }
        Exception Exception { get; }
    }
}
