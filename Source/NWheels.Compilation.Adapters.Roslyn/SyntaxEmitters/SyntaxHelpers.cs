using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public static class SyntaxHelpers
    {
        private static readonly IReadOnlyDictionary<Type, SyntaxKind> _s_keywordPerType = new Dictionary<Type, SyntaxKind> {
            [typeof(bool)] = SyntaxKind.BoolKeyword,
            [typeof(byte)] = SyntaxKind.ByteKeyword,
            [typeof(sbyte)] = SyntaxKind.SByteKeyword,
            [typeof(short)] = SyntaxKind.ShortKeyword,
            [typeof(ushort)] = SyntaxKind.UShortKeyword,
            [typeof(int)] = SyntaxKind.IntKeyword,
            [typeof(uint)] = SyntaxKind.UIntKeyword,
            [typeof(long)] = SyntaxKind.LongKeyword,
            [typeof(ulong)] = SyntaxKind.ULongKeyword,
            [typeof(double)] = SyntaxKind.DoubleKeyword,
            [typeof(float)] = SyntaxKind.FloatKeyword,
            [typeof(decimal)] = SyntaxKind.DecimalKeyword,
            [typeof(string)] = SyntaxKind.StringKeyword,
            [typeof(char)] = SyntaxKind.CharKeyword,
            [typeof(object)] = SyntaxKind.ObjectKeyword,
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
            else if (value is bool boolValue)
            {
                return LiteralExpression(boolValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
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

        public static TypeSyntax GetTypeNameSyntax(this TypeMember type)
        {
            if (type.ClrBinding != null)
            {
                if (type.ClrBinding.IsArray)
                {
                    var elementTypeSyntax = GetTypeNameSyntax(type.UnderlyingType);
                    return ArrayType(elementTypeSyntax)
                        .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))));
                }

                if (_s_keywordPerType.TryGetValue(type.ClrBinding, out SyntaxKind keyword))
                {
                    return PredefinedType(Token(keyword));
                }

                if (IsNullableValueType(type.ClrBinding.GetTypeInfo(), out Type underlyingValueType))
                {
                    return NullableType(GetTypeNameSyntax(underlyingValueType));
                }
            }

            return GetTypeFullNameSyntax(type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static NameSyntax GetTypeFullNameSyntax(this TypeMember type)
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

        public static ArgumentListSyntax GetArgumentListSyntax(this MethodCallExpression call)
        {
            IEnumerable<ArgumentSyntax> argumentSyntaxes = call.Arguments.Select(EmitArgument);
            return ArgumentList(SeparatedList(argumentSyntaxes));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BlockSyntax ToSyntax(this BlockStatement block)
        {
            if (block != null)
            {
                return Block(block.Statements.Select(StatementSyntaxEmitter.EmitSyntax));
            }

            return Block();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetValidCSharpIdentifier(string proposedName)
        {
            if (string.IsNullOrEmpty(proposedName) || SyntaxFacts.IsValidIdentifier(proposedName))
            {
                return proposedName;
            }

            var validatedName = new char[proposedName.Length];

            for (int i = 0 ; i < proposedName.Length ; i++)
            {
                var c = proposedName[i];

                if (!char.IsLetter(c) && !(char.IsDigit(c) && i > 0) && !(c == '_'))
                {
                    validatedName[i] = '_';
                }
                else
                {
                    validatedName[i] = c;
                }
            }

            return new string(validatedName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static NameSyntax QualifyTypeNameSyntax(TypeMember type, SimpleNameSyntax simpleName)
        {
            if (type.DeclaringType != null)
            {
                return QualifiedName(GetTypeFullNameSyntax(type.DeclaringType), simpleName);
            }

            if (!string.IsNullOrEmpty(type.Namespace))
            {
                var isNamespaceImported = type.SafeBackendTag().IsNamespaceImported;

                if (!isNamespaceImported)
                {
                    return QualifiedName(ParseName(type.Namespace), simpleName);
                }
            }

            return simpleName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsNullableValueType(System.Reflection.TypeInfo typeInfo, out Type underlyingValueType)
        {
            if (typeInfo.IsGenericType && 
                !typeInfo.IsGenericTypeDefinition && 
                typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                underlyingValueType = typeInfo.GetGenericArguments()[0];
                return true;
            }

            underlyingValueType = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ArgumentSyntax EmitArgument(Argument argument)
        {
            var syntax = Argument(ExpressionSyntaxEmitter.EmitSyntax(argument.Expression));

            switch (argument.Modifier)
            {
                case MethodParameterModifier.Ref:
                    syntax = syntax.WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword));
                    break;
                case MethodParameterModifier.Out:
                    syntax = syntax.WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));
                    break;
            }

            return syntax;
        }
    }
}
