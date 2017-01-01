using NWheels.Api.Compilation.Syntax.Expressions;
using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface IThrowStatement : IStatement
    {
        ITypeMember ExceptionType { get; }
        IReadOnlyList<IExpression> ConstructorArguments { get; }
    }
}
