using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class LockStatement : AbstractStatement
    {
        public LockStatement()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression SyncRoot { get; set; }
        public AbstractExpression EnterTimeout { get; set; }
        public BlockStatement Body { get; set; }
    }
}
