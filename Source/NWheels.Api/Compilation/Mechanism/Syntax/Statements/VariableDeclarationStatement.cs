using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class VariableDeclarationStatement : AbstractStatement
    {
        public LocalVariable Variable { get; set; }
        public AbstractExpression InitialValue { get; set; } 
    }
}
