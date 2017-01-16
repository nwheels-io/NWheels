using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public abstract class MemberSyntaxEmitterBase<TMember, TSyntax> : SyntaxEmitterBase<TSyntax>
        where TMember : AbstractMember
        where TSyntax : MemberDeclarationSyntax
    {
        protected MemberSyntaxEmitterBase(TMember member)
        {
            this.Member = member;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TMember Member { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected SyntaxList<AttributeListSyntax> EmitAttributeLists()
        {
            return SingletonList<AttributeListSyntax>(
                AttributeList(
                    SeparatedList<AttributeSyntax>(
                        Member.Attributes.Select(SyntaxHelpers.ToAttributeSyntax))));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected SyntaxTokenList EmitVisibilityModifiers()
        {
            return TokenList(_s_visibilityKeywords[Member.Visibility].Select(Token));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IReadOnlyDictionary<MemberVisibility, SyntaxKind[]> _s_visibilityKeywords =
            new Dictionary<MemberVisibility, SyntaxKind[]> {
                [MemberVisibility.Public] = new[] { SyntaxKind.PublicKeyword },
                [MemberVisibility.Protected] = new[] { SyntaxKind.ProtectedKeyword },
                [MemberVisibility.Internal] = new[] { SyntaxKind.InternalKeyword },
                [MemberVisibility.InternalProtected] = new[] { SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword },
                [MemberVisibility.Private] = new[] { SyntaxKind.PrivateKeyword }
            };
    }
}
