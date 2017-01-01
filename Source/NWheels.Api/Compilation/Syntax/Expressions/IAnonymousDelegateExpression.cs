using NWheels.Api.Compilation.Syntax.Members;
using NWheels.Api.Compilation.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface IAnonymousDelegateExpression
    {
        IMethodSignature Signature { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        IStatement Body { get; }
    }
}
