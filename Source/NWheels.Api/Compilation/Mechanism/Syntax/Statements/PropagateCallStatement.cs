using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class PropagateCallStatement : AbstractStatement
    {
        public override void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitPropagateCallStatement(this);

            if (Target != null)
            {
                Target.AcceptVisitor(visitor);
            }

            if (ReturnValue != null)
            {
                visitor.VisitReferenceToLocalVariable(ReturnValue);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Target { get; set; }

        //TODO: what is this variable for?
        public LocalVariable ReturnValue { get; set; } 
    }
}
