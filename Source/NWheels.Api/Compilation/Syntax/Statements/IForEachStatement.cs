using NWheels.Api.Compilation.Syntax.Expressions;
using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IForEachStatement : IStatement
    {
        IExpression Enumerable { get; }
        ILocalVariable CurrentItem { get; }
    }
}
