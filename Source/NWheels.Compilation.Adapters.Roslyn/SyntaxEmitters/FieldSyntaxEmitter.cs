using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class FieldSyntaxEmitter : MemberSyntaxEmitterBase<FieldMember, FieldDeclarationSyntax>
    {
        public FieldSyntaxEmitter(FieldMember field)
            : base(field)
        {
        }

        public override FieldDeclarationSyntax EmitSyntax()
        {
            throw new NotImplementedException();
        }
    }
}