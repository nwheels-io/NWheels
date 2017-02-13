using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class NewObjectExpression : AbstractExpression
    {
        public NewObjectExpression()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitNewObjectExpression(this);

            if (ConstructorCall != null)
            {
                ConstructorCall.AcceptVisitor(visitor);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodCallExpression ConstructorCall { get; set; }
    }
}
