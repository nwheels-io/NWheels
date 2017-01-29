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
    public static class SyntaxHelpers
    {
        public static AttributeSyntax ToAttributeSyntax(this AttributeDescription description)
        {
            return AttributeSyntaxEmitter.EmitSyntax(description);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ExpressionSyntax GetLiteralSyntax(object value)
        {
            if (value is ConstantExpression expression)
            {
                value = expression.Value;
            }
            if (value == null)
            {
                return LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            else if (value is int intValue)
            {
                return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(intValue));
            }
            else if (value is float floatValue)
            {
                return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(floatValue));
            }
            else if (value is long longValue)
            {
                return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(longValue));
            }
            else if (value is decimal decimalValue)
            {
                return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(decimalValue));
            }
            else if (value is uint uintValue)
            {
                return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(uintValue));
            }
            else if (value is double doubleValue)
            {
                return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(doubleValue));
            }
            else if (value is ulong ulongValue)
            {
                return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(ulongValue));
            }
            else if (value is string stringValue)
            {
                return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(stringValue));
            }
            else if (value is char charValue)
            {
                return LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal(charValue));
            }
            else if (value is Type typeValue)
            {
                return TypeOfExpression(GetTypeNameSyntax(typeValue));
            }
            else if (value is TypeMember typeMember)
            {
                return TypeOfExpression(GetTypeNameSyntax(typeMember));
            }

            throw new NotSupportedException($"Literals of type {value.GetType().Name} are not supported");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static NameSyntax GetTypeNameSyntax(TypeMember type)
        {
            if (!type.IsGenericType)
            {
                return QualifyTypeNameSyntax(type, IdentifierName(type.Name));
            }

            var genericSyntax = GenericName(type.Name)
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SeparatedList<TypeSyntax>(
                            type.GenericTypeArguments.Select(GetTypeNameSyntax))));

            return QualifyTypeNameSyntax(type, genericSyntax);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static NameSyntax QualifyTypeNameSyntax(TypeMember type, SimpleNameSyntax simpleName)
        {
            if (type.DeclaringType != null)
            {
                return QualifiedName(GetTypeNameSyntax(type.DeclaringType), simpleName);
            }

            if (!string.IsNullOrEmpty(type.Namespace))
            {
                return QualifiedName(ParseName(type.Namespace), simpleName);
            }

            return simpleName;
        }
    }
}
