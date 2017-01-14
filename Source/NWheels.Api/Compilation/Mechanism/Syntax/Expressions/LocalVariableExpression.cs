using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class LocalVariableExpression : AbstractExpression
    {
        public LocalVariable Variable { get; set; }
    }
}
