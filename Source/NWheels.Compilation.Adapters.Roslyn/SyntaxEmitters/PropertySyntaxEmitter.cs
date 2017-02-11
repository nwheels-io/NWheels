using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class PropertySyntaxEmitter : MemberSyntaxEmitterBase<PropertyMember, PropertyDeclarationSyntax>
    {
        public PropertySyntaxEmitter(PropertyMember property)
            : base(property)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override PropertyDeclarationSyntax EmitSyntax()
        {
            OutputSyntax = PropertyDeclaration(
                Member.PropertyType.GetTypeNameSyntax(),
                Identifier(Member.Name)
            );

            OutputSyntax = OutputSyntax.WithModifiers(EmitMemberModifiers());

            OutputSyntax = OutputSyntax.WithAccessorList(
                AccessorList(
                    SingletonList<AccessorDeclarationSyntax>(
                        AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration
                        )
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)
                        )
                    )
                )
            );

            return OutputSyntax;
        }
    }
}