using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class BlockStatement : AbstractStatement
    {
        public BlockStatement(params AbstractStatement[] statements)
        {
            this.Body = new List<AbstractStatement>(statements);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<AbstractStatement> Body { get; private set; }
    }
}
