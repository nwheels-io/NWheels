using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class ConstructorSyntaxEmitter : MethodMemberSyntaxEmitterBase<ConstructorMember, ConstructorDeclarationSyntax>
    {
        public ConstructorSyntaxEmitter(ConstructorMember constructor) 
            : base(constructor)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ConstructorDeclarationSyntax EmitSyntax()
        {
            OutputSyntax = ConstructorDeclaration(Identifier(Member.Name));

            if (Member.Modifier != MemberModifier.Static)
            {
                OutputSyntax = OutputSyntax.WithModifiers(EmitMemberVisibility());

                if (Member.Signature.Parameters.Count > 0)
                {
                    OutputSyntax = OutputSyntax.WithParameterList(MethodSignatureSyntaxEmitter.EmitParameterListSyntax(Member.Signature));
                }
            }
            else
            {
                OutputSyntax = OutputSyntax.WithModifiers(TokenList(Token(SyntaxKind.StaticKeyword)));
            }

            if (Member.CallThisConstructor != null)
            {
                OutputSyntax = OutputSyntax.WithInitializer(
                    ConstructorInitializer(
                        SyntaxKind.ThisConstructorInitializer,
                        Member.CallThisConstructor.GetArgumentListSyntax()));
            }
            else if (Member.CallBaseConstructor != null)
            {
                OutputSyntax = OutputSyntax.WithInitializer(
                    ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        Member.CallBaseConstructor.GetArgumentListSyntax()));
            }

            OutputSyntax = OutputSyntax.WithBody(Member.Body.ToSyntax());

            return OutputSyntax;
        }
    }
}