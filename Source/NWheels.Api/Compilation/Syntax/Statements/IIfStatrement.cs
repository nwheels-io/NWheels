using NWheels.Api.Compilation.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IIfStatrement : IStatement
    {
        IExpression Condition { get; }
        IStatement ThenBlock { get; }
        IStatement ElseBlock { get; }
    }
}
