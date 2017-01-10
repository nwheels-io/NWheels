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
            this.ConstructorArguments = new List<AbstractExpression>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<AbstractExpression> ConstructorArguments { get; private set; }
    }
}
