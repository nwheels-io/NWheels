using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;

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
            var declarator = VariableDeclarator(Identifier(Member.Name));

            if (Member.Initializer != null)
            {
                declarator = declarator.WithInitializer(EqualsValueClause(ExpressionSyntaxEmitter.EmitSyntax(Member.Initializer)));
            }

            OutputSyntax =
                FieldDeclaration(
                    VariableDeclaration(
                        Member.Type.GetTypeNameSyntax()
                    )
                    .WithVariables(
                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                            declarator
                        )
                    )
                )
                .WithModifiers(
                    EmitMemberModifiers()
                );

            //TODO: emit attributes

            return OutputSyntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<SyntaxKind> GetMemberModifierKeywords()
        {
            if (Member.IsReadOnly)
            {
                return base.GetMemberModifierKeywords().Append(SyntaxKind.ReadOnlyKeyword);
            }

            return base.GetMemberModifierKeywords();
        }
    }
}