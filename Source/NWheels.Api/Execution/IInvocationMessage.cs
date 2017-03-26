using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace NWheels.Execution
{
    public interface IInvocationMessage
    {
        void Invoke(object target);
        Type TargetType { get; }
        MethodInfo TargetMethod { get; }
        object Result { get; }
        Exception Exceprion { get; }
    }
}
