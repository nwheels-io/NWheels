using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public static class StatementSyntaxEmitter
    {
        public static StatementSyntax EmitSyntax(AbstractStatement statement)
        {
            if (statement is ReturnStatement ret)
            {
                return ReturnStatement(ExpressionSyntaxEmitter.EmitSyntax(ret.Expression));
            }
            if (statement is BlockStatement block)
            {
                return block.ToSyntax();
            }

            //TODO: support other types of statements

            throw new NotSupportedException($"Syntax emitter is not supported for statement of type '{statement.GetType().Name}'.");
        }
    }
}
