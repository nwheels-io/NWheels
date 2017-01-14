using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class CastExpression : AbstractExpression
    {
        public AbstractExpression Source { get; set; }
    }
}
