using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            if (expression is LocalVariableExpression local)
            {
                return IdentifierName(local.Variable.Name);
            }
            if (expression is MethodCallExpression call)
            {
                return EmitMethodCallSyntax(call);
            }
            if (expression is NewObjectExpression newObject)
            {
                return EmitNewObjectSyntax(newObject);
            }
            if (expression is ThisExpression)
            {
                return ThisExpression();
            }
            if (expression is BaseExpression)
            {
                return BaseExpression();
            }
            if (expression is ArgumentExpression argument)
            {
                return IdentifierName(argument.Parameter.Name);
            }

            //TODO: support other types of expressions

            throw new NotSupportedException($"Syntax emitter is not supported for expression node of type '{expression.GetType().Name}'.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ExpressionSyntax EmitNewObjectSyntax(NewObjectExpression newObject)
        {
            var syntax = ObjectCreationExpression(newObject.Type.GetTypeNameSyntax());

            if (newObject.ConstructorCall != null)
            {
                syntax = syntax.WithArgumentList(newObject.ConstructorCall.GetArgumentListSyntax());
            }
            else
            {
                syntax = syntax.WithArgumentList(ArgumentList());
            }

            return syntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ExpressionSyntax EmitMethodCallSyntax(MethodCallExpression call)
        {
            InvocationExpressionSyntax syntax;
            var methodIdentifier = IdentifierName(call.MethodName ?? call.Method.Name);

            if (call.Target != null)
            {
                syntax = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        EmitSyntax(call.Target),
                        methodIdentifier));
            }
            else
            {
                syntax = InvocationExpression(methodIdentifier);
            }

            if (call.Arguments.Count > 0)
            {
                syntax = syntax.WithArgumentList(call.GetArgumentListSyntax());
            }

            return syntax;
        }
    }
}
