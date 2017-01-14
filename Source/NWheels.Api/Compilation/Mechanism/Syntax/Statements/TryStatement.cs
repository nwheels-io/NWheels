using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class TryStatement : AbstractStatement
    {
        public TryStatement()
        {
            this.TryBlock = new BlockStatement();
            this.CatchBlocks = new List<TryCatchBlock>();
            this.FinallyBlock = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BlockStatement TryBlock { get; private set; }
        public List<TryCatchBlock> CatchBlocks { get; private set; }
        public BlockStatement FinallyBlock { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TryCatchBlock
    {
        public TryCatchBlock()
        {
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember ExceptionType { get; set; } 
        public LocalVariable ExceptionVariable { get; set; }
        public BlockStatement Body { get; private set; }
    }
}
