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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override MethodDeclarationSyntax EmitSyntax()
        {
            TypeSyntax returnTypeSyntax = (Member.Signature.IsVoid
                ? PredefinedType(Token(SyntaxKind.VoidKeyword))
                : Member.Signature.ReturnValue.Type.GetTypeNameSyntax());

            OutputSyntax =
                MethodDeclaration(
                    returnTypeSyntax,
                    Identifier(Member.Name)
                );

            OutputSyntax = OutputSyntax.WithModifiers(EmitMemberModifiers());

            if (Member.Signature.Parameters.Count > 0)
            {
                OutputSyntax = OutputSyntax.WithParameterList(MethodSignatureSyntaxEmitter.EmitParameterListSyntax(Member.Signature));
            }

            OutputSyntax = OutputSyntax.WithBody(Block());

            return OutputSyntax;
        }
    }
}