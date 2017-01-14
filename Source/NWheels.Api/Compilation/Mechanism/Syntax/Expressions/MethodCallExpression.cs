using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class MethodCallExpression : AbstractExpression
    {
        public MethodCallExpression()
        {
            this.Arguments = new List<AbstractExpression>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Target { get; set; }
        public MethodMember Method { get; set; }
        public string MethodName { get; set; }
        public List<AbstractExpression> Arguments { get; private set; }
        public bool IsAsyncAwait { get; set; }
    }
}

