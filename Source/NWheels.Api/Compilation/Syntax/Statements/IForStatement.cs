using NWheels.Api.Compilation.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IForStatement : IStatement
    {
        IReadOnlyList<IStatement> Initializer { get; }
        IExpression Condition { get; }
        IReadOnlyList<IStatement> Iterator { get; }
        IStatement Body { get; }
    }
}
