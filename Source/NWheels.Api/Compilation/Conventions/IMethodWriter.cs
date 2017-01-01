using NWheels.Api.Compilation.Syntax.Expressions;
using NWheels.Api.Compilation.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Conventions
{
    public interface IMethodWriter
    {
        void AddStatement(IStatement statement);
        IExpression THIS();
    }
}
