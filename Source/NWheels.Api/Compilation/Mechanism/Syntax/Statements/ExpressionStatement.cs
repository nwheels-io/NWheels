using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class ExpressionStatement : AbstractStatement
    {
        public AbstractExpression Expression { get; set; }
    }
}
