using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class ClassSyntaxEmitter : TypeMemberSyntaxEmitterBase<TypeMember, ClassDeclarationSyntax>
    {
        public ClassSyntaxEmitter(TypeMember member) 
            : base(member)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ClassDeclarationSyntax EmitSyntax()
        {
            OutputSyntax = ClassDeclaration(Member.Name);

            if (Member.Attributes.Count > 0)
            {
                OutputSyntax = OutputSyntax.WithAttributeLists(EmitAttributeLists());
            }

            OutputSyntax = OutputSyntax.WithModifiers(EmitMemberModifiers());

            if (Member.BaseType != null || Member.Interfaces.Count > 0)
            {
                OutputSyntax = OutputSyntax.WithBaseList(EmitBaseList());
            }

            OutputSyntax = OutputSyntax.WithMembers(EmitMembers());

            return OutputSyntax;
        }
    }
}
