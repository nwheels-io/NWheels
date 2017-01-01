using NWheels.Api.Compilation.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IDoWhileStatement : IStatement
    {
        IStatement Body { get; }
        IExpression Condition { get; }
    }
}
