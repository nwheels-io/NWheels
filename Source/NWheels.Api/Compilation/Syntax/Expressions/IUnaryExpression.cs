using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface IUnaryExpression : IExpression
    {
        UnaryOperator Operator { get; }
        IExpression Operand { get; }
    }
}
