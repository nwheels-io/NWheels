using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class DoStatement : AbstractStatement
    {
        public DoStatement()
        {
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BlockStatement Body { get; private set; }
        public AbstractExpression Condition { get; set; }
    }
}
