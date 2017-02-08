using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class AssignmentExpression : AbstractExpression
    {
        public AbstractExpression Left { get; set; }
        public AbstractExpression Right { get; set; }
    }
}
