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

        public override void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitUsingStatement(this);

            if (Disposable != null)
            {
                Disposable.AcceptVisitor(visitor);
            }

            Body.AcceptVisitor(visitor);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Disposable { get; set; }
        public BlockStatement Body { get; }
    }
}
