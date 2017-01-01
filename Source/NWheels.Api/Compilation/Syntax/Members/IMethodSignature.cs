using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface IMethodSignature
    {
        bool IsVoid { get; }
        bool IsAsync { get; }
        bool IsReturnByRef { get; }
        ITypeMember ReturnType { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        MethodBase Binding { get; }
    }
}
