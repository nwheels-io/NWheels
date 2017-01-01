using NWheels.Api.Compilation.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IUsingStatement : IStatement
    {
        IExpression Disposable { get; }
        IStatement Body { get; }
    }
}
