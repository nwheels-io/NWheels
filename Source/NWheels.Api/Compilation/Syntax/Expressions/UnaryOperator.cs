using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public enum UnaryOperator
    {
        LogicalNot,
        BitwiseNot,
        Plus,
        Negation,
        PreIncrement,
        PostIncrement,
        PreDecrement,
        PostDecrement
    }
}
