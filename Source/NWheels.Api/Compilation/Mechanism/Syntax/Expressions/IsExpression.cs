using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class IsExpression : AbstractExpression
    {
        public AbstractExpression Source { get; set; }
        public LocalVariable PatternMatchVariable { get; set; }
    }
}
