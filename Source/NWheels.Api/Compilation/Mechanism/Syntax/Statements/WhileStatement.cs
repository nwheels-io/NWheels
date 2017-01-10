using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class WhileStatement : AbstractStatement
    {
        public WhileStatement()
        {
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Condition { get; set; }
        public BlockStatement Body { get; private set; }
    }
}
