using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class ParameterExpression : AbstractExpression
    {
        public MethodParameter Parameter { get; set; }
    }
}
