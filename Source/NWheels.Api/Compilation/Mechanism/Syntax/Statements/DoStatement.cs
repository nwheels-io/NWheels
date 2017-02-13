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

        public override void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitDoStatement(this);

            Body.AcceptVisitor(visitor);

            if (Condition != null)
            {
                Condition.AcceptVisitor(visitor);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BlockStatement Body { get; }
        public AbstractExpression Condition { get; set; }
    }
}
