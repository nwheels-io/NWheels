using NWheels.Api.Compilation.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IWhileStatement : IStatement
    {
        IExpression Condition { get; }
        IStatement Body { get; }
    }
}
