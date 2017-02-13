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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitTryStatement(this);

            TryBlock.AcceptVisitor(visitor);

            foreach (var catchBlock in CatchBlocks)
            {
                catchBlock.AcceptVisitor(visitor);
            }

            if (FinallyBlock != null)
            {
                FinallyBlock.AcceptVisitor(visitor);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BlockStatement TryBlock { get; }
        public List<TryCatchBlock> CatchBlocks { get; }
        public BlockStatement FinallyBlock { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TryCatchBlock
    {
        public TryCatchBlock()
        {
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AcceptVisitor(StatementVisitor visitor)
        {
            if (ExceptionType != null)
            {
                visitor.VisitReferenceToTypeMember(ExceptionType);
            }

            if (ExceptionVariable != null)
            {
                visitor.VisitReferenceToLocalVariable(ExceptionVariable);
            }

            Body.AcceptVisitor(visitor);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember ExceptionType { get; set; } 
        public LocalVariable ExceptionVariable { get; set; }
        public BlockStatement Body { get; }
    }
}
