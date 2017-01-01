using NWheels.Api.Compilation.Syntax.Expressions;
using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface ISwitchStatement : IStatement
    {
        IReadOnlyList<ISwitchCaseBlock> CaseBlocks { get; }
    }

    public interface ISwitchCaseBlock
    {
        IExpression ConstantMatch { get; }
        ITypeMember PatternMatchType { get; }
        IExpression PatternMatchCondition { get; }
        IStatement Body { get; }        
    }
}
