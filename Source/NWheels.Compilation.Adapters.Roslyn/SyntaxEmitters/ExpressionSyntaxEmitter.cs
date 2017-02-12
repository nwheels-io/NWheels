using Microsoft.CodeAnalysis;
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
            if (expression is ParameterExpression argument)
            {
                return IdentifierName(argument.Parameter.Name);
            }
            if (expression is AssignmentExpression assignment)
            {
                return AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    EmitSyntax(assignment.Left),
                    EmitSyntax(assignment.Right));
            }
            if (expression is MemberExpression member)
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    EmitSyntax(member.Target),
                    IdentifierName(member.Member?.Name ?? member.MemberName));
            }
            if (expression is NewArrayExpression newArray)
            {
                return EmitNewArraySyntax(newArray);
            }
            if (expression is IndexerExpression indexer)
            {
                return ElementAccessExpression(EmitSyntax(indexer.Target)).WithArgumentList(
                    BracketedArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            indexer.IndexArguments.Select(arg => Argument(EmitSyntax(arg))))));
            }
            if (expression is BinaryExpression binary)
            {
                return BinaryExpression(GetBinaryOperatorKeyword(binary.Operator), EmitSyntax(binary.Left), EmitSyntax(binary.Right));
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

        private static ExpressionSyntax EmitNewArraySyntax(NewArrayExpression newArray)
        {
            SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = (
                newArray.DimensionLengths.Count > 0
                ? List(newArray.DimensionLengths.Select(dimLen => ArrayRankSpecifier(SingletonSeparatedList(EmitSyntax(dimLen)))))
                : SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))
            );

            var syntax = ArrayCreationExpression(
                ArrayType(newArray.ElementType.GetTypeNameSyntax()).WithRankSpecifiers(rankSpecifiers)
            );

            //TODO: support multi-dimensional array initializers
            if (newArray.DimensionInitializerValues.Count > 0)
            {
                syntax = syntax.WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList(newArray.DimensionInitializerValues[0].Select(EmitSyntax))));
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static SyntaxKind GetBinaryOperatorKeyword(BinaryOperator op)
        {
            switch (op)
            {
                case BinaryOperator.Equal:
                    return SyntaxKind.EqualsExpression;
                case BinaryOperator.NotEqual:
                    return SyntaxKind.NotEqualsExpression;
                case BinaryOperator.GreaterThan:
                    return SyntaxKind.GreaterThanExpression;
                case BinaryOperator.LessThan:
                    return SyntaxKind.LessThanExpression;
                //TODO: include the rest of the cases
                default:
                    throw new NotSupportedException($"Binary operator '{op}' is not supported.");
            }
        }
    }
}
