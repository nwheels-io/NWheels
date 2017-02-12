using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public abstract class TypeMemberSyntaxEmitterBase<TMember, TSyntax> : MemberSyntaxEmitterBase<TMember, TSyntax>
        where TMember : TypeMember
        where TSyntax : BaseTypeDeclarationSyntax
    {
        protected TypeMemberSyntaxEmitterBase(TMember member)
            : base(member)
        {
            if (member.Generator.FactoryType != null && member.Generator.TypeKey.HasValue)
            {
                AddTypeKeyAttribute();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void AddTypeKeyAttribute()
        {
            var typeKeyAttribute = new AttributeDescription() {
                AttributeType = typeof(TypeKeyAttribute)
            };
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.FactoryType });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.Value.PrimaryContract });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.Value.SecondaryContract1 });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.Value.SecondaryContract2 });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.Value.SecondaryContract3 });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.Value.ExtensionValue1 });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.Value.ExtensionValue2 });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.Value.ExtensionValue3 });

            Member.Attributes.Add(typeKeyAttribute);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected BaseListSyntax EmitBaseList()
        {
            var baseList = new List<BaseTypeSyntax>();

            if (Member.BaseType != null)
            {
                baseList.Add(ToBaseTypeSyntax(Member.BaseType));
            }

            baseList.AddRange(Member.Interfaces.Select(ToBaseTypeSyntax));

            return BaseList(SeparatedList<BaseTypeSyntax>(baseList));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected BaseTypeSyntax ToBaseTypeSyntax(TypeMember baseTypeMember)
        {
            return SimpleBaseType(SyntaxHelpers.GetTypeFullNameSyntax(baseTypeMember));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected SyntaxList<MemberDeclarationSyntax> EmitMembers()
        {
            var orderedMembers = new List<AbstractMember>(Member.Members);
            orderedMembers.Sort(new MemberOrderComparer());

            return List<MemberDeclarationSyntax>(orderedMembers
                .Select(m => CreateMemberSyntaxEmitter(m).EmitSyntax())
                .Cast<MemberDeclarationSyntax>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ISyntaxEmitter CreateMemberSyntaxEmitter(AbstractMember member)
        {
            if (member is FieldMember field)
            {
                return new FieldSyntaxEmitter(field);
            }

            if (member is ConstructorMember constructor)
            {
                return new ConstructorSyntaxEmitter(constructor);
            }

            if (member is MethodMember method)
            {
                return new MethodSyntaxEmitter(method);
            }

            if (member is PropertyMember property)
            {
                return new PropertySyntaxEmitter(property);
            }

            if (member is EventMember @event)
            {
                return new EventSyntaxEmitter(@event);
            }

            throw new ArgumentException($"Syntax emitter is not supported for members of type {member.GetType().Name}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class MemberOrderComparer : IComparer<AbstractMember>
        {
            public int Compare(AbstractMember x, AbstractMember y)
            {
                var orderIndexX = GetMemberOrderIndex(x);
                var orderIndexY = GetMemberOrderIndex(y);

                return orderIndexY.CompareTo(orderIndexX);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static int GetMemberOrderIndex(AbstractMember member)
            {
                var memberTypeIndex = GetMemberTypeIndex(member);
                var visibilityIndex = GetMemberVisibilityIndex(member);
                var modifierIndex = GetMemberModifierIndex(member);

                return modifierIndex + visibilityIndex + memberTypeIndex;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static int GetMemberModifierIndex(AbstractMember member)
            {
                switch (member.Modifier)
                {
                    case MemberModifier.Static:
                        return 10000;
                    default:
                        return 20000;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static int GetMemberVisibilityIndex(AbstractMember member)
            {
                switch (member.Visibility)
                {
                    case MemberVisibility.Public:
                        return 500;
                    case MemberVisibility.InternalProtected:
                        return 400;
                    case MemberVisibility.Protected:
                        return 300;
                    case MemberVisibility.Internal:
                        return 200;
                    case MemberVisibility.Private:
                        return 100;
                    default:
                        return 0;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static int GetMemberTypeIndex(AbstractMember member)
            {
                if (member is FieldMember)
                {
                    return 2000;
                }
                if (member is ConstructorMember)
                {
                    return 1000;
                }
                if (member is MethodMember)
                {
                    return 80;
                }
                if (member is PropertyMember)
                {
                    return 70;
                }
                if (member is EventMember)
                {
                    return 60;
                }

                return 0;
            }
        }
    }
}
