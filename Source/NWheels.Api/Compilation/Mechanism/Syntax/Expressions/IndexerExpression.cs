using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class IndexerExpression : AbstractExpression
    {
        public IndexerExpression()
        {
            this.IndexArguments = new List<AbstractExpression>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Target { get; set; }
        public List<AbstractExpression> IndexArguments { get; private set; }
    }
}
