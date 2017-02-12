using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class IfStatement : AbstractStatement
    {
        public IfStatement()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Condition { get; set; }
        public BlockStatement ThenBlock { get; set; }
        public BlockStatement ElseBlock { get; set; }
    }
}
