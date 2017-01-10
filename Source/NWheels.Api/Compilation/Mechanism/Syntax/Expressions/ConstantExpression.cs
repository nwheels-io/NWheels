using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class ConstantExpression : AbstractExpression
    {
        public object Value { get; set; }
    }
}
