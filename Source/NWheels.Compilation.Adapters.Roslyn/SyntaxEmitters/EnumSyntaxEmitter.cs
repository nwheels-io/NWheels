using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class EnumSyntaxEmitter : MemberSyntaxEmitterBase<TypeMember, EnumDeclarationSyntax>
    {
        public EnumSyntaxEmitter(TypeMember member)
            : base(member)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override EnumDeclarationSyntax EmitSyntax()
        {
            OutputSyntax = EnumDeclaration(Member.Name);

            if (Member.Attributes.Count > 0)
            {
                OutputSyntax = OutputSyntax.WithAttributeLists(EmitAttributeLists());
            }

            OutputSyntax = OutputSyntax.WithModifiers(EmitVisibilityModifiers());
            OutputSyntax = OutputSyntax.WithMembers(EmitEnumMembers());

            return OutputSyntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private SeparatedSyntaxList<EnumMemberDeclarationSyntax> EmitEnumMembers()
        {
            return SeparatedList(
                Member.Members.OfType<EnumMember>()
                    .Select(ToEnumMemberSyntax));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EnumMemberDeclarationSyntax ToEnumMemberSyntax(EnumMember member)
        {
            var syntax = EnumMemberDeclaration(Identifier(member.Name));

            if (member.Value != null)
            {
                syntax = syntax.WithEqualsValue(EqualsValueClause(SyntaxHelpers.GetLiteralSyntax(member.Value)));
            }

            return syntax;
        }
    }
}
