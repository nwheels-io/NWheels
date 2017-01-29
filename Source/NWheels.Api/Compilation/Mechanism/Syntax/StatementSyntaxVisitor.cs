using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax
{
    public abstract class StatementSyntaxVisitor
    {
        public virtual void VisitAbstractStatement(AbstractStatement statement)
        {
        }

        public virtual void VisitBlockStatement(BlockStatement statement)
        {
        }
    }
}
