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
        Task<object> Invoke(object target);
        Task<object> Awaitable { get; }
        TaskAwaiter<object> GetAwaiter();
        Type TargetType { get; }
        MethodInfo TargetMethod { get; }
        object Result { get; }
        Exception Exception { get; }
    }
}
