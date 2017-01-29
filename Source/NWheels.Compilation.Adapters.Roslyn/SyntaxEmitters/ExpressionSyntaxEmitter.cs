using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public static class ExpressionSyntaxEmitter
    {
        public static ExpressionSyntax EmitSyntax(AbstractExpression expression)
        {
            if (expression is ConstantExpression constant)
            {
                return SyntaxHelpers.GetLiteralSyntax(constant.Value);
            }

            //TODO: support other types of expressions

            throw new NotSupportedException($"Syntax emitter is not supported for expression node of type '{expression.GetType().Name}'.");
        }
    }
}
