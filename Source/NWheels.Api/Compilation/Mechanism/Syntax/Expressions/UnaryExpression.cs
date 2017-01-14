using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class UnaryExpression : AbstractExpression
    {
        public UnaryOperator Operator { get; set; }
        public AbstractExpression Operand { get; set; }
    }
}
