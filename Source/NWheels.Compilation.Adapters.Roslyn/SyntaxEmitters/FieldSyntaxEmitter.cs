using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class FieldSyntaxEmitter : MemberSyntaxEmitterBase<FieldMember, FieldDeclarationSyntax>
    {
        public FieldSyntaxEmitter(FieldMember field)
            : base(field)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override FieldDeclarationSyntax EmitSyntax()
        {
            OutputSyntax =
                FieldDeclaration(
                    VariableDeclaration(
                        Member.Type.GetTypeNameSyntax()
                    )
                    .WithVariables(
                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                            VariableDeclarator(
                                Identifier(Member.Name)
                            )
                        )
                    )
                )
                .WithModifiers(
                    EmitMemberModifiers()
                );

            //TODO: emit attributes

            return OutputSyntax;
        }
    }
}