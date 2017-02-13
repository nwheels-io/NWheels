using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public abstract class AbstractStatement
    {
        public abstract void AcceptVisitor(StatementVisitor visitor);
    }
}
