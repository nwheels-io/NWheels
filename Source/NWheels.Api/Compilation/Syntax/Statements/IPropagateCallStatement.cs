using NWheels.Api.Compilation.Syntax.Expressions;
using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IPropagateCallStatement
    {
        IExpression Target { get; }
        ILocalVariable ReturnValue { get; }
    }
}
