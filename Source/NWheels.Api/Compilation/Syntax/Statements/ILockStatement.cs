using NWheels.Api.Compilation.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface ILockStatement : IStatement
    {
        IExpression SyncRoot { get; }
        IExpression EnterTimeout { get; }
        IStatement Body { get; }
    }
}
