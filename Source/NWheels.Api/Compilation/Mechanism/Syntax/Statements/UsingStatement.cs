using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class UsingStatement : AbstractStatement
    {
        public UsingStatement()
        {
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Disposable { get; set; }
        public BlockStatement Body { get; private set; }
    }
}
