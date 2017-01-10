using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class ForStatement : AbstractStatement
    {
        public ForStatement()
        {
            this.Initializers = new List<AbstractStatement>();
            this.Iterators = new List<AbstractStatement>();
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<AbstractStatement> Initializers { get; private set; }
        public AbstractExpression Condition { get; set; }
        public List<AbstractStatement> Iterators { get; private set; }
        public BlockStatement Body { get; private set; }
    }
}
