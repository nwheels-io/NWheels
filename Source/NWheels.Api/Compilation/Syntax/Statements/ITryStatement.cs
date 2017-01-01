using NWheels.Api.Compilation.Syntax.Expressions;
using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Statements
{
    public interface ITryStatement : IStatement
    {
        IStatement TryBlock { get; }
        IReadOnlyList<ITryCatchBlock> CatchBlocks { get; }
        IStatement FinallyBlock { get; }
    }

    public interface ITryCatchBlock
    {
        ITypeMember ExceptionType { get; } 
        ILocalVariable ExceptionVariable { get; }
        IStatement CatchStatement { get; }
    }
}
