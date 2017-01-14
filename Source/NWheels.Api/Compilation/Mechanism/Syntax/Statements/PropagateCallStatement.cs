using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class PropagateCallStatement : AbstractStatement
    {
        public AbstractExpression Target { get; set; }
        public LocalVariable ReturnValue { get; set; }
    }
}
