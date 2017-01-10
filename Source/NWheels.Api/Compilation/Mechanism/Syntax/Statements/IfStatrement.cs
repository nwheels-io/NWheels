using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class IfStatrement : AbstractStatement
    {
        public IfStatrement()
        {
            this.ThenBlock = new BlockStatement();
            this.ElseBlock = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Condition { get; set; }
        public BlockStatement ThenBlock { get; private set; }
        public BlockStatement ElseBlock { get; private set; }
    }
}
