using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class BinaryExpression : AbstractExpression
    {
        public AbstractExpression Left { get; set; }
        public BinaryOperator Operator { get; set; }
        public AbstractExpression Right { get; set; }
    }
}
