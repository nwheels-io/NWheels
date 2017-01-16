using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class ConstructorSyntaxEmitter : MethodMemberSyntaxEmitterBase<ConstructorMember, ConstructorDeclarationSyntax>
    {
        public ConstructorSyntaxEmitter(ConstructorMember constructor) 
            : base(constructor)
        {
        }

        public override ConstructorDeclarationSyntax EmitSyntax()
        {
            throw new NotImplementedException();
        }
    }
}