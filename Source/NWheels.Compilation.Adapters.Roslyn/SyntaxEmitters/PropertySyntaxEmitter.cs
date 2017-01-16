using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class PropertySyntaxEmitter : MemberSyntaxEmitterBase<PropertyMember, PropertyDeclarationSyntax>
    {
        public PropertySyntaxEmitter(PropertyMember property)
            : base(property)
        {
        }

        public override PropertyDeclarationSyntax EmitSyntax()
        {
            throw new NotImplementedException();
        }
    }
}