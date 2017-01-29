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
            if (member.Generator.FactoryType != null && member.Generator.TypeKey != null)
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
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.PrimaryContract });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = null }); //TODO: pass array of secondary contract types
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = Member.Generator.TypeKey.ExtensionType });
            typeKeyAttribute.ConstructorArguments.Add(new ConstantExpression() { Value = null }); //TODO: pass array of extension values

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
            return SimpleBaseType(SyntaxHelpers.GetTypeNameSyntax(baseTypeMember));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected SyntaxList<MemberDeclarationSyntax> EmitMembers()
        {
            return List<MemberDeclarationSyntax>(Member.Members
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
    }
}
