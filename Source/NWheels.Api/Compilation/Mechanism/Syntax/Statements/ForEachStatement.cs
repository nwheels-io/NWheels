using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class ForEachStatement : AbstractStatement
    {
        public ForEachStatement()
        {
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Enumerable { get; set; }
        public BlockStatement Body { get; private set; }
        public LocalVariable CurrentItem { get; set; }
    }
}
