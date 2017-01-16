using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public abstract class MethodMemberSyntaxEmitterBase<TMember, TSyntax> : MemberSyntaxEmitterBase<TMember, TSyntax>
        where TMember : MethodMemberBase
        where TSyntax : BaseMethodDeclarationSyntax
    {
        protected MethodMemberSyntaxEmitterBase(TMember member) 
            : base(member)
        {
        }
    }
}
