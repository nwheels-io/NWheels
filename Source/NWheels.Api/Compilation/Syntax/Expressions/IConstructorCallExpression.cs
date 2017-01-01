using System;
using System.Collections.Generic;
using System.Text;

 namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface IConstructorCallExpression : IExpression
    {
        IReadOnlyList<IExpression> Arguments { get; }
    }
}
