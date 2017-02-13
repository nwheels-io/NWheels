using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class ConstantExpression : AbstractExpression
    {
        public override void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitConstantExpression(this);

            if (Value != null)
            {
                visitor.VisitReferenceToTypeMember(Value.GetType());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object Value { get; set; }
    }
}
