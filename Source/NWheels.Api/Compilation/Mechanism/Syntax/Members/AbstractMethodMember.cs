using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public abstract class MethodMemberBase : AbstractMember
    {
        public MethodSignature Signature { get; set; }
        public AbstractStatement Body { get; set; }
    }
}
