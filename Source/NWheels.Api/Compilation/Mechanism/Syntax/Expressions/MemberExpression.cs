using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class MemberExpression : AbstractExpression
    {
        public AbstractExpression Target { get; set; }
        public AbstractMember Member { get; set; }
        public string MemberName { get; set; }
    }
}
