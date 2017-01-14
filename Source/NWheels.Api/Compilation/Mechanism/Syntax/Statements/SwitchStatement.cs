using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Statements
{
    public class SwitchStatement : AbstractStatement
    {
        public SwitchStatement()
        {
            this.CaseBlocks = new List<SwitchCaseBlock>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<SwitchCaseBlock> CaseBlocks { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SwitchCaseBlock
    {
        public SwitchCaseBlock()
        {
            this.Body = new BlockStatement();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression ConstantMatch { get; set; }
        public TypeMember PatternMatchType { get; set; }
        public AbstractExpression PatternMatchCondition { get; set; }
        public BlockStatement Body { get; private set; }        
    }
}
