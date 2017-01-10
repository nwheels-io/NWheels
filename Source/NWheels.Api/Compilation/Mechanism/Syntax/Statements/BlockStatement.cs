using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class BlockStatement : AbstractStatement
    {
        public BlockStatement()
        {
            this.Body = new List<AbstractStatement>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<AbstractStatement> Body { get; private set; }
    }
}
