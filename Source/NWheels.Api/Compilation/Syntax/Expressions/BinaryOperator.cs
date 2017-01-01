using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public enum BinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide, 
        Modulus,
        LogicalAnd,
        LogicalOr,
        LogicalXor,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        LeftShift,
        RightShift,
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        CastOrThrow,
        TryCast,
        NullCoalesce,
        Assign
    }
}
