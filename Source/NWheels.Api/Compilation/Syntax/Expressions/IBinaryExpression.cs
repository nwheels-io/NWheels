using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface IBinaryExpression : IExpression
    {
        IExpression Left { get; }
        BinaryOperator Operator { get; }
        IExpression Right { get; }
    }
}
