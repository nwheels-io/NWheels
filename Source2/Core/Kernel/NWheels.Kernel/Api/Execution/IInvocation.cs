using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NWheels.Kernel.Api.Execution
{
    public interface IInvocation
    {
        Task Invoke(object target);
        Task Promise { get; }
        object Result { get; }
        Type TargetType { get; }
        MethodInfo TargetMethod { get; }
    }
}
