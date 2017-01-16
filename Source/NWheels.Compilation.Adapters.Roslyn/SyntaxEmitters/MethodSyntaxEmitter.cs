using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    internal class MethodSyntaxEmitter : MemberSyntaxEmitterBase<MethodMember, MethodDeclarationSyntax>
    {
        public MethodSyntaxEmitter(MethodMember method)
            : base(method)
        {
        }

        public override MethodDeclarationSyntax EmitSyntax()
        {
            OutputSyntax =
                MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)
                    ),
                    Identifier(Member.Name)
                );

            OutputSyntax = OutputSyntax.WithModifiers(EmitVisibilityModifiers());
            OutputSyntax = OutputSyntax.WithBody(Block());

            return OutputSyntax;
        }
    }
}