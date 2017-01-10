using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class ThrowStatement : AbstractStatement
    {
        public ThrowStatement()
        {
            this.ConstructorArguments = new List<AbstractExpression>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember ExceptionType { get; set; }
        public List<AbstractExpression> ConstructorArguments { get; private set; }
    }
}
